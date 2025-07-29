export function printLeaveRequest() {
    const content = document.querySelector('#leave-print-area');
    if (!content) return;

    const printWindow = window.open('', '_blank', 'width=800,height=1000');
    printWindow.document.write(`
        <html>
            <head>
                <title>Demande de Congé</title>
                <link rel="stylesheet" href="/css/attestation.css">
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        padding: 20px;
                    }
                    .signature-line {
                        margin-top: 60px;
                        border-top: 1px solid black;
                        width: 70%;
                        margin-left: auto;
                        margin-right: auto;
                    }
                </style>
            </head>
            <body onload="window.print(); window.close();">
                <div class="printable-content">
                    ${content.innerHTML}
                </div>
            </body>
        </html>
    `);
    printWindow.document.close();
}
