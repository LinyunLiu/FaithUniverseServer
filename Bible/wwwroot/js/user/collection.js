let CurrentIndex = 0;
let CardAndNote = []
let Owner = ""
let Origin = ""
function initialize(data, owner, origin){
    CardAndNote = data
    Owner = owner
    Origin = origin
}

function msg(t) {
    let text = document.getElementById("message-text")
    let message = document.getElementById("message")
    if (t === 'Saved!') {
        text.style.color = "lightgreen"
    }
    text.innerText = t;
    message.style.opacity = "1"
    setTimeout(function () {
        message.style.opacity = "0"
    }, 5000)
}
function handleEmptyCollection(){
    if (CardAndNote.length === 0) {
        let m = document.getElementById('side-bar-menu')
        let c = document.getElementById('content')
        m.style.display = "none"
        c.style.display = "none"
    }
    else{
        let p = document.getElementById('empty-collection')
        p.style.display = "none"
    }
}
function display(index){
    if (CurrentIndex !== index) {
        let note = document.getElementById("note")
        if (note.value.trim() !== ""){
            CardAndNote[CurrentIndex]['Note'] = note.value;
        }
        let c = document.getElementById(CurrentIndex.toString());
        let h = document.getElementById(index.toString());
        c.className = "not-chosen";
        h.className = "highlight"
        let t = document.getElementById('text');
        let v = document.getElementById('verse');
        let n = document.getElementById('note');
        t.innerText = CardAndNote[index]['Text'];
        v.innerText = CardAndNote[index]['Verse'];
        n.value = CardAndNote[index]['Note'];
        CurrentIndex = index;
    }
}
function next(){
    let toIndex = CurrentIndex + 1;
    if (toIndex === CardAndNote.length){
        toIndex = 0;
    }
    display(toIndex)
}
function last(){
    let toIndex = CurrentIndex - 1;
    if (toIndex < 0){
        toIndex = CardAndNote.length - 1;
    }
    display(toIndex)
}
function saveNote(){
    let note = document.getElementById("note")
    if (note.value.trim() !== ""){
        CardAndNote[CurrentIndex]["Content"] = note.value;
        const data = {
            Username: Owner,
            Content: note.value,
            ForVerseId: CardAndNote[CurrentIndex]['Id']
        };
        fetch(`${Origin}/user-collection/save-note`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
            .then(response => {
                if (response.ok){
                    msg("Saved!")
                }
            })
            .catch(() => {msg("Something Went Wrong")});
    }
}
function deleteNote(){
    if (confirm("Are you sure?")){
        const data = {
            Username: Owner,
            ForVerseId: CardAndNote[CurrentIndex]['Id']
        };
        fetch(`${Origin}/user-collection/delete-verse`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
            .then(response => {
                if (response.ok){
                    window.location.href = `${Origin}/user-collection`
                }
                else{
                    msg("Something Went Wrong, Please Try Again")
                }
            })
            .catch(() => {msg("Something Went Wrong, Please Try Again")});
    }
}