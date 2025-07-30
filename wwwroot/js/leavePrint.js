export function printLeaveRequest() {
    const content = document.querySelector('#leave-print-area');

    if (!content) return;

    const printWindow = window.open('', '_blank', 'width=800,height=1000');
    const stylesheets = Array.from(document.styleSheets)
        .map(styleSheet => {
            try {
                if (styleSheet.href) {
                    return `<link rel="stylesheet" href="${styleSheet.href}">`;
                }
            } catch (e) {
                console.warn("Cannot access stylesheet: ", styleSheet);
            }
            return '';
        })
        .join('\n');

    printWindow.document.write(`
        <html>
            <head>
                <title>Demande de Congé</title>
                ${stylesheets}
                <style>
                    /* Optional: Add inline styles for print-specific formatting */
                    @media print {
                        body {
                            margin: 0;
                            font-family: sans-serif;
                            color: #000;
                        }
                    }

                    /* Prevent print preview content from being cut off */
                    html, body {
                        height: auto !important;
                        overflow: visible !important;
                    }
                </style>
            </head>
            <body onload="window.print(); window.close();">
                ${content.outerHTML}
            </body>
        </html>
    `);

    printWindow.document.close();
}
