export function previewAttestation() {
    const content = document.querySelector('.printable-content');
    if (!content) return;

    const win = window.open('', '_blank', 'width=800,height=1000');
    win.document.write(`
        <html>
            <head>
                <title>Aperçu Attestation</title>
                <link rel="stylesheet" href="/css/attestation.css">
            </head>
            <body>
                <div class="printable-content">${content.innerHTML}</div>
            </body>
        </html>
    `);
    win.document.close();
}


export function printAttestationContent() {
    const content = document.querySelector('.printable-content');
    if (!content) return;

    const printWindow = window.open('', '', 'width=800,height=1000');
    printWindow.document.write(`
        <html>
            <head>
                <title>Impression Attestation</title>
                <link rel="stylesheet" type="text/css" href="/css/attestation.css" />
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

export function saveAttestationAsPdf(fileName = "attestation.pdf") {
    const element = document.querySelector('.printable-content');
    if (!element) return;

    import('https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js')
        .then(() => {
            html2pdf().set({
                margin: 10,
                filename: fileName,
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2 },
                jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
            }).from(element).save();
        });
}
export function previewLeave() {
    const content = document.querySelector('.printable-content');
    if (!content) return;

    const win = window.open('', '_blank', 'width=800,height=1000');
    win.document.write(`
        <html>
            <head>
                <title>Aperçu Attestation</title>
                <link rel="stylesheet" href="/css/attestation.css">
            </head>
            <body>
                <div class="printable-content">${content.innerHTML}</div>
            </body>
        </html>
    `);
    win.document.close();
}


export function printLeaveContent() {
    const content = document.querySelector('.printable-content');
    if (!content) return;

    const printWindow = window.open('', '', 'width=800,height=1000');
    printWindow.document.write(`
        <html>
            <head>
                <title>Impression Attestation</title>
                <link rel="stylesheet" type="text/css" href="/css/attestation.css" />
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