// site.js
window.getSelectedValues = function (selectElement) {
    return Array.from(selectElement.selectedOptions).map(o => parseInt(o.value));
};

function printSection(id) {
    const content = document.getElementById(id).innerHTML;

    // ADD THIS LINE TO LOG THE CONTENT
    console.log("Content to be printed:", content);

    const win = window.open('', '', 'width=800,height=1000');

    const stylesheets = Array.from(document.styleSheets)
        .map(style => {
            if (style.href) {
                return `<link rel="stylesheet" href="${style.href}" />`;
            }
            return ''; // skip inline
        })
        .join('\n');

    win.document.write(`
        <html>
        <head>
            <title>Fiche de paie</title>
            ${stylesheets}
            <style>
                @page {
                    size: A4;
                    margin: 0;
                }
                body {
                    margin: 0;
                    padding: 0;
                    font-family: 'Segoe UI', sans-serif;
                    font-size: 11pt;
                    background-color: white;
                    color: black;
                }

                .payroll-sheet {
                    width: 190mm;
                    min-height: 277mm;
                    padding: 10mm;
                    box-sizing: border-box;
                    margin: 0 auto;
                    background-color: white;
                }

                .no-print {
                    display: none !important;
                }
            </style>
        </head>
        <body onload="window.print(); window.close();">
            <div class="payroll-sheet">
                ${content}
            </div>
        </body>
        </html>
    `);

    win.document.close();
}
window.downloadFileFromStream = async (fileName, contentType, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: contentType });
    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? 'contract';
    anchorElement.click();
    anchorElement.remove();

    URL.revokeObjectURL(url);
};
