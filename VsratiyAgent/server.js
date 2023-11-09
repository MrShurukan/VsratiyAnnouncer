const d2gsi = require('dota2-gsi');
const server = new d2gsi();

let prevGold = 0;
let prevItemSet = null;

let pauseCount = 0;
let pauseCountTimeDecreaseHandler = null;

let useAbilityZeroHandler = null;
let recentlyUsedAbility0 = false;

server.events.on('newclient', function(client) {
    console.log("New client connection, IP address: " + client.ip);
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
        if (activity === 'playing') console.log("Game started!");
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