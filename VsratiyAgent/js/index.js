document.getElementById("pudgeImage").addEventListener("click", playPudgeVideo);
document.getElementById("pudgeVideo").addEventListener("ended", function () {
    document.getElementById("pudgeImage").classList.remove("hidden");
    this.classList.add("hidden");
});

function playPudgeSound() {
    const audio = new Audio("sounds/pudge_go_f_yourself.mp3");
    audio.volume = 0.35;
    audio.play();
}

function playPudgeVideo() {
    const image = document.getElementById("pudgeImage");
    const video = document.getElementById("pudgeVideo");

    image.classList.add("hidden");
    video.classList.remove("hidden");

    video.volume = 0.35;
    video.play();
}