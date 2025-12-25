// JavaScript functions for file operations in the RH application
// This file contains the missing JavaScript functions that the EmployeeFiles.razor component relies on

// Function to reset file input elements
window.resetFileInput = function (fileInput) {
    if (fileInput) {
        try {
            // Reset the file input by setting its value to empty string
            fileInput.value = '';
        } catch (error) {
            console.error('Error resetting file input:', error);
        }
    }
};

// Function to download files from base64 data
window.downloadFileFromBase64 = function (fileName, contentType, base64Data) {
    try {
        // Convert base64 to binary
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Create blob and download
        const blob = new Blob([bytes], { type: contentType });
        const url = URL.createObjectURL(blob);
        
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName || 'file';
        document.body.appendChild(anchorElement);
        anchorElement.click();
        document.body.removeChild(anchorElement);
        
        // Clean up the URL object
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Error downloading file:', error);
        // Fallback to creating a temporary download link
        try {
            const blob = new Blob([atob(base64Data)], { type: contentType });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName || 'file';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        } catch (fallbackError) {
            console.error('Fallback download also failed:', fallbackError);
        }
    }
};

// Fallback function for downloading files (in case downloadFileFromBase64 is not available)
window.downloadFileFallback = function (fileName, contentType, base64Data) {
    try {
        // Convert base64 to binary
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Create blob and download
        const blob = new Blob([bytes], { type: contentType });
        const url = URL.createObjectURL(blob);
        
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName || 'file';
        document.body.appendChild(anchorElement);
        anchorElement.click();
        document.body.removeChild(anchorElement);
        
        // Clean up the URL object
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Fallback download failed:', error);
        alert('Download failed. Please try again or contact support.');
    }
};
