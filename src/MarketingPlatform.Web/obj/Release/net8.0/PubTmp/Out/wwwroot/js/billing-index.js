/**
 * Billing Index Page - DataTables Implementation
 */

let billingTable;

$(document).ready(function() {
    initBillingTable();
});

function initBillingTable() {
    billingTable = $('#billingTable').DataTable({
        serverSide: true,
        processing: true,
        
        ajax: {
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.billing.invoices),
            type: 'POST',
            headers: getAjaxHeaders(),
            data: function(d) {
                return {
                    pageNumber: Math.floor(d.start / d.length) + 1,
                    pageSize: d.length,
                    searchTerm: d.search.value,
                    sortColumn: d.columns[d.order[0]?.column]?.data || 'invoiceDate',
                    sortDirection: d.order[0]?.dir || 'desc'
                };
            },
            dataSrc: function(json) {
                json.recordsTotal = json.totalCount || 0;
                json.recordsFiltered = json.totalCount || 0;
                return json.items || json.data?.items || [];
            }
        },
        
        columns: [
            {
                data: 'invoiceNumber',
                name: 'Invoice',
                orderable: true,
                searchable: true,
                render: function(data) {
                    return `<strong>${escapeHtml(data)}</strong>`;
                }
            },
            {
                data: 'invoiceDate',
                name: 'Date',
                orderable: true,
                searchable: false,
                render: function(data) {
                    return formatShortDate(data);
                }
            },
            {
                data: 'amount',
                name: 'Amount',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<strong>$${data.toFixed(2)}</strong>`;
                    }
                    return data;
                }
            },
            {
                data: 'status',
                name: 'Status',
                orderable: true,
                searchable: false,
                render: function(data) {
                    const statusColors = { 'Paid': 'success', 'Pending': 'warning', 'Overdue': 'danger', 'Cancelled': 'secondary' };
                    return createBadge(data, statusColors[data] || 'secondary');
                }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function(data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="viewInvoice(${data})"><i class="bi bi-eye"></i></button>
                            <button class="btn btn-outline-secondary" onclick="downloadInvoice(${data})"><i class="bi bi-download"></i></button>
                            ${row.status === 'Pending' ? `<button class="btn btn-outline-success" onclick="payInvoice(${data})"><i class="bi bi-credit-card"></i></button>` : ''}
                        </div>
                    `;
                }
            }
        ],
        
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[1, 'desc']],
        
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"Btr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        
        buttons: [
            { extend: 'csv', className: 'btn btn-sm btn-outline-primary me-1', text: '<i class="bi bi-file-earmark-csv"></i> CSV', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'excel', className: 'btn btn-sm btn-outline-success me-1', text: '<i class="bi bi-file-earmark-excel"></i> Excel', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'pdf', className: 'btn btn-sm btn-outline-danger', text: '<i class="bi bi-file-earmark-pdf"></i> PDF', exportOptions: { columns: ':visible:not(.no-export)' } }
        ],
        
        language: {
            processing: '<div class="spinner-border text-primary"></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No invoices found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ invoices'
        },
        
        stateSave: true,
        searchDelay: 300
    });
}

function viewInvoice(id) { window.location.href = AppUrls.billing?.invoice ? AppUrls.billing.invoice(id) : `/Billing/Invoice/${id}`; }

function downloadInvoice(id) {
    showNotification('Downloading invoice...', 'info');
    window.location.href = window.AppUrls.buildApiUrl(window.AppUrls.api.billing.downloadInvoice(id));
}

function payInvoice(id) {
    window.location.href = AppUrls.billing?.pay ? AppUrls.billing.pay(id) : `/Billing/Pay/${id}`;
}
