function tg(){
    let loader = document.getElementById("loader")
    loader.style.opacity = "1"
}
function msg() {
    let text = document.getElementById("message-text")
    let message = document.getElementById("message")
    if (text.innerText.trim() !== "") {
        if (text.innerText.trim() === "AI Responded!") {
            text.style.color = "lightgreen"
        }
        message.style.opacity = "1"
        setTimeout(function () {
            message.style.opacity = "0"
        }, 15000)
    }
    else{
        message.style.display = "none"
    }
}
function tgAdd(i, mode){
    // mode 1:loading | 2:successful | 3:failed
    let icon = document.getElementById(`add-indicate-${i}`);
    if (mode === 1){
        icon.className = 'add-loader';
        icon.style.pointerEvents = "none";
    }
    else if (mode === 2){
        icon.className = 'bi bi-check-circle-fill';
        icon.style.color = "lightgreen"
        icon.style.pointerEvents = "none";
    }
    else{
        icon.className = 'bi bi-x-circle-fill';
        icon.style.color = "indianred"
        icon.style.pointerEvents = "none";
        setTimeout(function(){
            icon.className = 'bi bi-plus-circle';
            icon.style.color = "#ffd479"
            icon.style.pointerEvents = "all";
        }, 4000)
    }
}