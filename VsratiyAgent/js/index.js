const fs = require('fs');
const repl = require("repl");
const isLocalServer = true;

const d2gsi = require('dota2-gsi');
const dotaGsiServer = new d2gsi();

window.addEventListener("keyup", keyListener, false);
function keyListener(e) {
    if (e.key === "F5") {
        register(e.ctrlKey);
    }
}

const id = fs.readFileSync("id.txt").toString();
const serverApiUrl = isLocalServer ? `http://localhost:5064/agent/${id}/` : `https://vsratiyannouncer/agent/${id}/`;

var app = new Vue({
    el: '#app',
    data: {
        id,
        isConnected: false,
        agentObject: { personName: "Не загружено" },
        isDota2Launched: false,
        isGameInProgress: false,
        isInDiscordWithABot: false,
        isLoading: true,
        events: [],

        IMG_TICK: 'img/success.png',
        IMG_CROSS: 'img/failed.png',
        INFO_EVENT_TYPE   : 0,
        DANGER_EVENT_TYPE : 1
    },
    methods: {
        getTickOrCross(condition) {
            return condition ? this.IMG_TICK : this.IMG_CROSS;
        },

        addEvent(eventType, message) {
            const date = new Date();
            const time= `${this.beautifyNumber(date.getHours())}:${this.beautifyNumber(date.getMinutes())}`;

            this.events.push({ type: eventType, time, message });
        },

        beautifyNumber(number) {
            return number < 10 ? "0" + number : number;
        },

        clickPudge() {
            playPudgeVideo();
        },

        stopPudgeVideo() {
            document.getElementById("pudgeImage").classList.remove("hidden");
            document.getElementById("pudgeVideo").classList.add("hidden");
        },

        startPingInterval() {
            setInterval(() => {
                if (this.isConnected) {
                    this.makePing();
                }
            }, 1000 * 10);
        },

        makePing() {
            fetch(serverApiUrl + "ping", {
                method: "POST"
            })
            .then(response => {
                if (response.ok)
                    return response.json();

                if (response.status === 418) {
                    this.addEvent(this.DANGER_EVENT_TYPE, "Сервер приказал отсоединиться");
                }
                else {
                    response.text().then(errText => {
                        this.addEvent(this.DANGER_EVENT_TYPE, `Произошла ошибка во время пинга: ${errText}`);
                    });
                }

                this.isConnected = false;
                this.agentObject.name = "Не подключено";
            })
            .then(agent => {
                this.updateAgentInfo(agent)
            })
            .catch(err => {
                this.addEvent(this.DANGER_EVENT_TYPE, `Произошла неивзестная ошибка во время пинга: ${err}`);

                this.isConnected = false;
                this.agentObject.name = "Не подключено";
            });
        },

        updateAgentInfo(agent) {
            this.agentObject = agent;

            this.isLoading = false;
            this.isConnected = true;
        }
    }
})

function register(force=false) {
    fetch(serverApiUrl + "register?force="+force, {
        method: "POST",
    })
    .then(response => {
        if (response.ok)
            return response.json();

        if (response.status === 418)
            throw new Error("Агент уже подключен с таким ID");

        response.text().then(text => {
            throw new Error(text);
        });
    })
    .then(agent => {
        app.updateAgentInfo(agent);

        app.addEvent(app.INFO_EVENT_TYPE, `Успешное подключение к серверу`);
    })
    .catch(err => {
        app.$data.isLoading = false;
        app.$data.name = `:(`;

        app.addEvent(app.DANGER_EVENT_TYPE, `Произошла ошибка во время подключения:\n${err.message}`);

        console.error("Ошибка!", err);
    });
}
register();
app.startPingInterval();

function playPudgeSound() {
    const audio = new Audio("sounds/pudge_go_f_yourself.mp3");
    audio.volume = 0.35;
    audio.play();
}

function playPudgeVideo() {
    console.log("click!");
    const image = document.getElementById("pudgeImage");
    const video = document.getElementById("pudgeVideo");

    image.classList.add("hidden");
    video.classList.remove("hidden");

    video.volume = 0.35;
    video.play();
}

/******************* DOTA 2 GSI *******************/
let prevGold = 0;
let prevItemSet = null;

let pauseCount = 0;
let pauseCountTimeDecreaseHandler = null;

let useAbilityZeroHandler = null;
let recentlyUsedAbility0 = false;

let lastClient;
setInterval(() => {
    if (lastClient?.gamestate?.hero?.id !== undefined) {
        app.$data.isDota2Launched = true;
        app.$data.isGameInProgress = true;
    }
    else {
        app.$data.isGameInProgress = false;
    }
}, 1000)

dotaGsiServer.events.on('newclient', function(client) {
    console.log("New client connection, IP address: " + client.ip);
    lastClient = client;

    app.$data.isDota2Launched = true;
    if (client.auth && client.auth.token) {
        console.log("Auth token: " + client.auth.token);
    } else {
        console.log("No Auth token");
    }

    client.on('map:game_state', function (gameState) {
        console.log("GameState:", gameState);
    });
    client.on('map:win_team', function (winTeam) {
        console.log("WinTeam:", winTeam);
    });
    client.on('player:activity', function(activity) {
        console.log("Activity:", activity);
        if (activity === 'playing') {
            console.log("Game started!");
            app.$data.isGameInProgress = true;
        }
    });
    client.on('map:paused', function (isPaused) {
        if (isPaused) {
            pauseCount++;
            console.log("Inc",pauseCount);

            if (pauseCountTimeDecreaseHandler != null) clearTimeout(pauseCountTimeDecreaseHandler);
            pauseCountTimeDecreaseHandler = setTimeout(pauseTimeout, 5000);

            if (pauseCount === 3) {
                console.log("Stop with the pauses!!!");
            }
        }
    });
    client.on('hero:level', function(level) {
        console.log("Now level " + level);
        if (level === 6) console.log("Congrats on the ult!!!");
    });
    client.on('abilities:ability0:can_cast', function(can_cast) {
        if (!can_cast) {
            console.log("Used ability 0!");
            recentlyUsedAbility0 = true;

            if (useAbilityZeroHandler != null) clearTimeout(useAbilityZeroHandler);
            useAbilityZeroHandler = setTimeout(() => recentlyUsedAbility0 = false, 2000);
        }
    });
    client.on('abilities:ability2:can_cast', function(can_cast) {
        if (!can_cast) {
            console.log("Used ability 2!");
            if (recentlyUsedAbility0) {
                console.log("FIRST THEN THIRD!!!");
                recentlyUsedAbility0 = false;
                clearTimeout(useAbilityZeroHandler);
            }
        }
    });
    client.on('hero:health_percent', function (percent) {
        if (percent < 20) console.log("You are dying!!");
        if (percent <= 0) console.log("You're dead...");
    });
    client.on('map:ward_purchase_cooldown', function (cooldown) {
        console.log("ward_cooldown", cooldown);
    });
    client.on('map:win_team', function (winTeam) {
        console.log("Someone won!", winTeam);
        console.log("Player team", client.gamestate.player.team_name);
        if (client.gamestate.player.team_name === winTeam)
            console.log("Player won!");
        else
            console.log("Player lost!");
    });
    client.on('map:clock_time', function (clock) {
        // console.log(clock);
        // console.log(client.gamestate.items);
        // console.log(client.gamestate.player);

        const itemSet = makeIntoItemSet(client.gamestate.items);
        if (prevItemSet != null) {
            if (!prevItemSet.equals(itemSet)) {
                const newItems = itemSet.findNew(prevItemSet);

                if (newItems.length > 0) {
                    const gold = client.gamestate.player.gold;
                    if (gold < prevGold) {
                        const percentLoss = ((prevGold-gold)/prevGold) * 100;
                        console.log(`Loss of gold by ${percentLoss}%`);

                        if (percentLoss > 70) {
                            if (newItems.length === 1) {
                                console.log("You sold your ass for", newItems[0].name);
                            }
                            else {
                                console.log("You sold your ass for these", newItems);
                            }
                        }
                    }
                }

            }
        }

        prevItemSet = itemSet;
    });
    client.on('player:gold', function (gold) {
        prevGold = gold;
    });
    client.on('player:kills', function (kills) {
        console.log(`New kill! Kills: ${kills}`);
    });
});

function makeIntoItemSet(items) {
    const set = [];
    for (const itemsKey in items) {
        const item = items[itemsKey];
        if (item.name === "empty") continue;

        if (set.includes(x => x.name === item.name)) continue;
        set.push(item);
    }

    set.sort(x => x.name);
    return new ItemSet(set);
}
function pauseTimeout() {
    pauseCount--;
    console.log("Dec",pauseCount);
    if (pauseCount > 0)
        pauseCountTimeDecreaseHandler = setTimeout(pauseTimeout, 5000);
}

class ItemSet {
    items = [];
    itemNames = [];

    constructor(items) {
        this.items = items;
        this.itemNames = items.map(x => x.name);
    }

    equals(itemSet2) {
        return arraysEqual(this.itemNames, itemSet2.itemNames);
    }

    findNew(itemSet2) {
        const newItems = []
        for (const item of this.items) {
            if (!itemSet2.itemNames.includes(item.name)) {
                newItems.push(item);
            }
        }
        return newItems;
    }

    findMissing(itemSet2) {
        return itemSet2.findNew(this);
    }
}

function arraysEqual(a, b) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length !== b.length) return false;

    for (var i = 0; i < a.length; ++i) {
        if (a[i] !== b[i]) return false;
    }
    return true;
}