let Origin = ""
let Username = ""
function initialize(origin, username) {
    Origin = origin
    Username = username
}
function toggle(val){
    let w = document.getElementById("delete");
    let c = document.getElementById("content");
    if (val){
        w.style.display = "flex";
        c.style.display = "none";
    }
    else{
        w.style.display = "none";
        c.style.display = "flex";
    }
}
function dA(){
    let input = document.getElementById("input");
    if (input.value.toString().trim() === "Delete My Account"){
        let data = {
            Username: Username,
            Email: "",
            Origin: ""
        }
        fetch(`${Origin}/delete-account`,{
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
            .then(response => {
                if (response.ok){
                    window.location.href = `${Origin}`
                }
            }).catch(err => {
                console.log(err)
        })
    }
}