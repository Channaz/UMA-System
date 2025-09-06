// wwwroot/js/blazor-js-utils.js

window.blazorJsUtils = {
    isSocketFunctionsLoaded: () => {
        return typeof window.socketFunctions !== 'undefined';
    }
};