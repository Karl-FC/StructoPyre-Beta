mergeInto(LibraryManager.library, {

    IsMobileBrowser: function() {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;

        // Check for mobile keywords
        if (/Mobi|Android|iPhone|iPad|iPod|windows phone/i.test(userAgent)) {
            // Additionally, check screen size as some tablets might not include "Mobi"
            // You might adjust these pixel values based on your target devices
            if (window.innerWidth <= 800 && window.innerHeight <= 600) {
                 return 1; // True if mobile keyword AND smallish screen
            }
            // Check specifically for iPad, which might not have 'Mobi' but is mobile
            if (/iPad/i.test(userAgent)) {
                 return 1; // True for iPad
            }
            // Consider it mobile if it has a mobile keyword but maybe a larger screen (tablet?)
            // Uncomment the line below if you want larger tablets to also trigger mobile mode
            // return 1; 
        }

        // If none of the above conditions matched, assume it's not mobile
        return 0; // False
    }

});
