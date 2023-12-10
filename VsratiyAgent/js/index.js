const fs = require('fs');
const repl = require("repl");
const isLocalServer = true;

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