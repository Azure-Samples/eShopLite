window.scrollToBottom = (elementId, pageId) => {
    console.log('scroll to botom for ' + elementId + " - pageId: " + pageId);

    // get the element by the elementId
    var element = document.getElementById(elementId);
    var page = document.getElementById(pageId);

    // if the element exists, scroll to the bottom using the page as the main container
    if (element) {
        element.scrollTop = page.scrollHeight;
    }
};

