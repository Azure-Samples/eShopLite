window.scrollToBottom = (element) => {
    console.log('scroll to botom for ' + element);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
    // force the page to scroll to the bottom
    window.scrollTo(0, window.document.body.scrollHeight + 120);
};

