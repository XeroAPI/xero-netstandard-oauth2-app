  /* --- GLOBAL VARS ---------------------------------------------------- */
var activeSideBarLinks = []
var sideBarLinks = document.getElementsByClassName("link-holder");

 /* --- ON PAGE LOAD ---------------------------------------------------- */
UpdateScrollPos();
UpdateSideNavBar();

window.onbeforeunload  = () => {
    SetActiveNavBarLinks();
    localStorage.setItem('scrollPos', JSON.stringify(GetScrollPos()));
}

 /* --- FUNCTIONS ------------------------------------------------------- */
// Generic method to add an item to an array
function addToArray(array, item){
    if (array == null || item == null) return;
    try{
        if (!array.includes(item))
            array.push(item);
            console.log(array);
    }catch{
        console.error("Attempting array operations on non-array object");
    }
}

// Generic method to remove an item to an array
function removeFromArray(array, item){
    if (array == null || item == null) return;
    try{
        var index = array.indexOf(item);
        if (index > -1){
            array.splice(index, 1);
        }
    }catch{
        console.error("Attempting array operations on non-array object");
    }
}

// Updates the background colour of side navbar links when selected
function UpdateLinkHolderColour(linkHolder){
    var index = 0;
    for(var i = 0; i < sideBarLinks.length; i++){
        if (sideBarLinks[i].isSameNode(linkHolder)){
            index = i;
            break;
        }
    }

    if (linkHolder.getAttribute('transition') != null){
        element.removeProperty('transition')
    }

    var closed =  linkHolder.getAttribute('aria-expanded');
    if (closed == 'false') {
        addToArray(activeSideBarLinks, index);
        linkHolder.setAttribute("id", "link-holder-selected");
    }else{
        removeFromArray(activeSideBarLinks, index);
        linkHolder.removeAttribute('id', 'link-holder-selected');
    }
}

// Ensure scroll position remains when page updating
function UpdateScrollPos(){
    if (localStorage.getItem('scroll-scrollPos') == null)
        var scrollPos = 0;
    else scrollPos = JSON.parse(localStorage.getItem('scrollPos'));   
    // Change scroll position
    $(window).scrollTop(scrollPos);
}

function GetScrollPos(){
    return $(window).scrollTop();
}

// Get all currently opened collapsible fields 
function SetActiveNavBarLinks(){
    activeSideBarLinks = [];

    for(var i = 0; i < sideBarLinks.length; i++){
        var closed = sideBarLinks[i].getAttribute('aria-expanded');
        if (closed == 'true'){
            // This link is currently selected
            activeSideBarLinks.push(i);
        }
    }
    localStorage.setItem('activeLinks', JSON.stringify(activeSideBarLinks));
}

// Open collapsible fields recorded in activeSideBarLinks
function UpdateSideNavBar(){

    if (localStorage.getItem('activeLinks') != null){
        activeSideBarLinks = JSON.parse(localStorage.getItem('activeLinks'));

        var collapsibleFields = document.getElementsByClassName("collapse");

        // Not connected so links won't be there
        if (collapsibleFields.length <= 1) return;

        for(var i = 0; i < activeSideBarLinks.length; i++){
            sideBarLinks[activeSideBarLinks[i]].setAttribute('aria-expanded', 'true');
            sideBarLinks[activeSideBarLinks[i]].setAttribute("id", "link-holder-selected");
            
            collapsibleFields[activeSideBarLinks[i] + 1].className = "collapse show"; 
        }

    }
}
