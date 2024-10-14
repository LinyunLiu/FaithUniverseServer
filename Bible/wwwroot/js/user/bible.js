let Bible = {} // All Bible Data from Server
let Origin = "" // Server Endpoint
let Username = "" // Username
let OldTestament = [] // And Array of BookDto
let NewTestament = [] // And Array of BookDto
let AllBooksName = [] // A String Arrays of all Book Names
let SearchResult = [] // Tmp storage to store Search Results
function initialize(bible, origin, username) {
    Bible = bible
    Origin = origin
    Username = username
    OldTestament = bible["OldTestament"]
    NewTestament = bible["NewTestament"]
    selectTestament("old")
    selectBook(0)
    selectChapter(1)
    fetchChapter(1)
    for (let book of OldTestament){
        AllBooksName.push(book["BookName"])
    }
    for (let book of NewTestament){
        AllBooksName.push(book["BookName"])
    }
    updateSearchResult()
}
let CurrentChosenTestament = "old" // old or new
let CurrentChosenBookIndex = 0 // Start from 0
let CurrentChosenChapterIndex = 1 // Start from 1
function displayTestament(testament, type){
    CurrentChosenBookIndex = 0
    CurrentChosenChapterIndex = 1
    let book_name = document.getElementById('book-name');
    let chapter = document.getElementById('chapter');
    book_name.innerHTML = ""
    chapter.innerHTML = ""
    let books = ""
    let chapters = ""
    for (let i = 0; i<testament.length; i++){
        books += `<p id="b-${i}" onclick="displayBook(${i})">${testament[i]["BookName"]}</p>`
    }
    for (let i=1; i<=testament[0]["ChapterCount"]; i++){
        chapters += `<p id="c-${i}" onclick="fetchChapter(${i})">Chapter ${i}</p>`
    }
    book_name.innerHTML = books;
    chapter.innerHTML = chapters;
    selectTestament(type)
    selectBook(0)
    selectChapter(1)
}
function displayBook(index){
    CurrentChosenChapterIndex = 1
    let chapter = document.getElementById('chapter');
    chapter.innerHTML = ""
    let chapters = ""
    if (CurrentChosenTestament === "old"){
        for (let i=1; i<=OldTestament[index]["ChapterCount"]; i++){
            chapters += `<p id="c-${i}" onclick="fetchChapter(${i})">Chapter ${i}</p>`
        }
    }
    else{
        for (let i=1; i<=NewTestament[index]["ChapterCount"]; i++){
            chapters += `<p id="c-${i}" onclick="fetchChapter(${i})">Chapter ${i}</p>`
        }
    }
    chapter.innerHTML = chapters;
    selectBook(index)
    selectChapter(1)
}
function fetchChapter(index){
    selectChapter(index)
    toggleChoosePanel(false)
    let ci = document.getElementById("chapter-indication")
    if (CurrentChosenTestament === "old"){
        let name = OldTestament[CurrentChosenBookIndex]["BookName"]
        ci.innerText = `${name} - Chapter ${index}`
        getChapter(name, index)
    }
    else{
        let name = NewTestament[CurrentChosenBookIndex]["BookName"]
        ci.innerText = `${name} - Chapter ${index}`
        getChapter(name, index)
    }
}
function selectTestament(type){
    let old_testament = document.getElementById('old-testament');
    let new_testament = document.getElementById('new-testament');
    if (type === "old"){
        old_testament.style.color = "indianred"
        new_testament.style.color = "#0c0c0c"
        CurrentChosenTestament = "old"
    }
    else{
        old_testament.style.color = "#0c0c0c"
        new_testament.style.color = "indianred"
        CurrentChosenTestament = "new"
    }
}
function selectBook(newIndex){
    let currentSelection = document.getElementById(`b-${CurrentChosenBookIndex.toString()}`);
    let newSelection = document.getElementById(`b-${newIndex.toString()}`);
    currentSelection.style.color = "#0c0c0c"
    newSelection.style.color = "indianred"
    CurrentChosenBookIndex = newIndex;
}
function selectChapter(newIndex){
    let currentSelection = document.getElementById(`c-${CurrentChosenChapterIndex.toString()}`);
    let newSelection = document.getElementById(`c-${newIndex.toString()}`);
    currentSelection.style.color = "#0c0c0c"
    newSelection.style.color = "indianred"
    CurrentChosenChapterIndex = newIndex;
}
function getChapter(bookName, chapterNumber){
    fetch(`${Origin}/user-bible/chapter?bookName=${encodeURIComponent(bookName)}&chapterNumber=${chapterNumber}`)
        .then(response => {
            if (response.ok) {
                return response.json()
            }
        })
        .then(data => {
            let container = document.getElementById('bible-text');
            container.innerHTML = "";
            let count = 1;
            for (let v of data) {
                container.innerHTML += 
                    `<p onclick="onVerseClicked('${bookName}', '${chapterNumber}', ${count})"><span class="verse-number">${count}</span> ${v.toString().replace("¶", "").replace('‹', '"').replace('›', '"')}</p>`
                count+=1;
            }
        })
        .catch(err => {
            console.log(err);
        })
}
let bookNameToAdd = "Genesis"
let chapterNumberToAdd = 1
let verseNumberToAdd = 1
function onVerseClicked(bookName, chapterNumber, verseNumber){
    let msg = document.getElementById("add-verse-info");
    msg.innerText = `Add ${bookName} ${chapterNumber}:${verseNumber} to your list?`;
    toggleAddVerseWindow(true)
    bookNameToAdd = bookName.toString();
    chapterNumberToAdd = parseInt(chapterNumber);
    verseNumberToAdd = verseNumber
}
function toggleAddVerseWindow(val){
    let window = document.getElementById("add-verse")
    window.style.display = val ? 'flex' : 'none';
}
function addVerse(){
    let url = `${Origin}/user-bible/add?username=${Username}&bookName=${encodeURIComponent(bookNameToAdd)}&chapterNumber=${chapterNumberToAdd}&verseNumber=${verseNumberToAdd}`
    fetch (url, {
        method: "POST",
    })
        .then(response => {
            toggleAddVerseWindow(false)
            if (response.ok){
                msg("Verse Added!")
            }
            else{
                msg("Something Went Wrong, Please Try Again")
            }
        })
        .catch(() => {
            msg("Something Went Wrong, Please Try Again")
        })
}
function msg(t) {
    let text = document.getElementById("message-text")
    let message = document.getElementById("message")
    if (t === 'Verse Added!') {
        text.style.color = "lightgreen"
    }
    text.innerText = t;
    message.style.opacity = "1"
    setTimeout(function () {
        message.style.opacity = "0"
    }, 5000)
}
function updateSearchResult(){
    let search = document.getElementById("search-input").value.trim();
    let container = document.getElementById("result")
    container.innerHTML = "";
    let names = findMatchName(removeIntegers(search).replace(" ", "").trim());
    let chapters = findIntegers(search)
    SearchResult = getPossibleResult(names, chapters)
    let count = 0;
    for (let r of SearchResult){
        container.innerHTML +=
            `<p onmousedown="fetchAndDisplayBookFromResult('${r[0]}', ${r[1]})"><i class="bi bi-arrow-right-short"></i> ${r[0]}: <span>Chapter ${r[1]}</span></p>`
        if (count === 9){
            break
        }
        count+=1;
    }
}
function findMatchName(input){
    let matches = []
    for (let name of AllBooksName) {
         if (name.toString().toLowerCase().includes(input.toLowerCase()) || input.toLowerCase().includes(name.toString().toLowerCase())){
             matches.push(name)
         }
    }
    return matches
}
function findIntegers(str) {
    let integers = str.match(/-?\d+/g);
    return integers ? integers.map(Number) : [];
}
function removeIntegers(str) {
    return str.replace(/\d+/g, '');
}
function findMax(name){
    for (let book of OldTestament){
        if (book["BookName"] === name){
            return book["ChapterCount"]
        }
    }
    for (let book of NewTestament){
        if (book["BookName"] === name){
            return book["ChapterCount"]
        }
    }
    return 1;
}
function getPossibleResult(names, chapters){
    let result = []
    if (chapters.length === 0){
        for (let name of names) {
            result.push([name, 1])
        }
    }
    else{
        for (let chapter of chapters) {
            for (let name of names) {
                let max = findMax(name);
                if (chapter <= max){
                    result.push([name, chapter])
                }
            }
        }
    }
    return result;
}
function fetchAndDisplayBookFromResult(bookName, chapterNumber){
    let ci = document.getElementById("chapter-indication")
    ci.innerText = `${bookName} - Chapter ${chapterNumber}`
    getChapter(bookName, chapterNumber)
}
function toggleSearchPanel(val){
    let panel = document.getElementById('search-panel');
    let bible = document.getElementById('bible');
    panel.style.display = val ? 'flex' : 'none';
    bible.style.display = val ? 'none' : 'flex';
}
function toggleChoosePanel(val){
    let panel = document.getElementById('choose-panel');
    let bible = document.getElementById('bible');
    panel.style.display = val ? 'flex' : 'none';
    bible.style.display = val ? 'none' : 'flex';
}
let isExpand = false
function expandBible(){
    let bible = document.getElementById('bible');
    let expand = document.getElementById('expand');
    if (!isExpand){
        expand.className = "bi bi-arrows-angle-contract expand"
        bible.className = "bible expand"
        isExpand = true
    }
    else{
        expand.className = "bi bi-arrows-angle-expand expand"
        bible.className = "bible not-expand"
        isExpand = false
    }
}