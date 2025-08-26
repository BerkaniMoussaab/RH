
    window.printDiv = (divId) => {
        let content = document.getElementById(divId).innerHTML;
    let printWindow = window.open("", "_blank", "width=800,height=600");

    // Custom payroll print CSS
    let styles = `
    <style>
        .payroll-sheet {
            width: 148mm; /* A5 width */
        min-height: auto;
        max-height: none;
        padding: 15mm;
        background-color: #fff;
        color: #333;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        font-size: 9pt;
        line-height: 1.4;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        margin: 20px auto;
        overflow: visible;
        }

        .company-logo {max - width: 100px; height: auto; }
        .section-title {font - size: 1em; font-weight: 600; color: #0056b3; border-bottom: 1px solid #eee; padding-bottom: 3px; margin-bottom: 10px; }
        .salary-summary-section .summary-item {background - color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 4px; padding: 7px 10px; text-align: center; margin-bottom: 8px; }
        .salary-summary-section .summary-item .label {font - size: 0.8em; color: #555; margin-bottom: 2px; }
        .salary-summary-section .summary-item .value {font - size: 1.1em; font-weight: 700; color: #333; }

        .net-pay-box {background - color: #e0f7fa; border: 2px solid #00bcd4; border-radius: 6px; padding: 10px 12px; text-align: center; margin-top: 15px; }
        .net-pay-label {font - size: 0.9em; font-weight: 600; color: #007bff; margin-bottom: 3px; }
        .net-pay-value {font - size: 1.8em; font-weight: 800; color: #0056b3; }

        .table-container {max - height: none; overflow: visible; page-break-inside: auto; }
        .details-table {width: 100%; border-collapse: collapse; margin-top: 5px; font-size: 0.85em; }
        .details-table th, .details-table td {padding: 5px 7px; border: 1px solid #dee2e6; vertical-align: top; }
        .details-table th {background - color: #f2f2f2; font-weight: 600; color: #495057; font-size: 0.8em; }
        .details-table tbody tr:nth-child(even) {background - color: #fefefe; }

        .description-cell {max - width: 100px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .footer-section {margin - top: 25px; padding-top: 12px; border-top: 1px solid #eee; font-size: 0.75em; color: #777; }
        .signature-label {font - weight: 600; margin-bottom: 8px; }
        .signature-line {border - bottom: 1px dashed #999; width: 130px; height: 25px; margin-left: auto; }
        .text-success {color: #28a745 !important; }
        .text-danger {color: #dc3545 !important; }

        @media print {
            body {margin: 0; padding: 0; }
        .payroll-sheet {width: 148mm; box-shadow: none; margin: 0; padding: 0; font-size: 8pt; }
        .table-container {max - height: none; overflow: visible; }
        .no-print {display: none !important; }
        .details-section, .footer-section, .section-title {page -break-inside: avoid; break-inside: avoid; }
        .details-table {font - size: 0.75em; }
        .net-pay-value {font - size: 1.6em; }
        .title {font - size: 1.4em; }
        }
    </style>
    `;

        // Also copy existing CSS (Bootstrap, etc.)
        document.querySelectorAll('link[rel="stylesheet"], style').forEach(node => {
        styles += node.outerHTML;
        });

    printWindow.document.write(`
    <html>
        <head>
            <title>Fiche de Paie</title>
            ${styles}
        </head>
        <body onload="window.print(); window.close();">
            ${content}
        </body>
    </html>
    `);
    printWindow.document.close();
    };
