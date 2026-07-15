// site.js - Centralized JavaScript for DİTİB Strasbourg ERP
// Consolidates personnel (Gorevli), leave (Izin), and placements (Gorevlendirme) scripts.

document.addEventListener("DOMContentLoaded", function () {
    // 1. Admin menu toggle logic
    const showAdminMenuBtn = document.getElementById("showAdminMenu");
    const backToStandardMenuBtn = document.getElementById("backToStandardMenu");
    const adminMenuContainer = document.getElementById("adminMenuContainer");
    const standardMenuItems = document.querySelectorAll(".standard-menu-item");
    const dernekGorevliCollapse = document.getElementById("dernekGorevliCollapse");

    if (showAdminMenuBtn && adminMenuContainer) {
        showAdminMenuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            standardMenuItems.forEach(item => item.classList.add("d-none"));
            if (dernekGorevliCollapse && dernekGorevliCollapse.classList.contains("show")) {
                let bsCollapse = bootstrap.Collapse.getInstance(dernekGorevliCollapse);
                if(bsCollapse) bsCollapse.hide();
            }
            adminMenuContainer.classList.remove("d-none");
            showAdminMenuBtn.closest('.mt-auto').classList.add("d-none");
        });
    }

    if (backToStandardMenuBtn && adminMenuContainer) {
        backToStandardMenuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            adminMenuContainer.classList.add("d-none");
            standardMenuItems.forEach(item => item.classList.remove("d-none"));
            showAdminMenuBtn.closest('.mt-auto').classList.remove("d-none");
        });
    }

    // 2. Izin Details Drag & Drop Event Listeners
    const dropZone = document.getElementById('dropZone');
    if (dropZone) {
        ['dragenter', 'dragover'].forEach(ev => {
            dropZone.addEventListener(ev, (e) => {
                e.preventDefault();
                dropZone.style.borderColor = '#6366f1';
                dropZone.style.background = '#f0f0ff';
            });
        });
        ['dragleave', 'drop'].forEach(ev => {
            dropZone.addEventListener(ev, (e) => {
                e.preventDefault();
                dropZone.style.borderColor = '#d1d5db';
                dropZone.style.background = '#f9fafb';
            });
        });
        dropZone.addEventListener('drop', (e) => {
            const file = e.dataTransfer.files[0];
            if (file) {
                const input = document.getElementById('evrakDosyasi');
                if (input) {
                    const dataTransfer = new DataTransfer();
                    dataTransfer.items.add(file);
                    input.files = dataTransfer.files;
                    if (typeof window.handleFileSelect === 'function') {
                        window.handleFileSelect(input);
                    }
                }
            }
        });
    }

    // 3. Izin Index Year Selector Change Event
    const yearSelector = document.getElementById('yearSelector');
    if (yearSelector) {
        yearSelector.addEventListener('change', function() {
            var selectedYear = this.value;
            var btnMerkez = document.getElementById('btnDownloadMerkez');
            if (btnMerkez) {
                var currentMerkezUrl = btnMerkez.getAttribute('href');
                if (currentMerkezUrl) {
                    btnMerkez.setAttribute('href', currentMerkezUrl.split('?')[0] + '?year=' + selectedYear);
                }
            }
            var btnImam = document.getElementById('btnDownloadImam');
            if (btnImam) {
                var currentImamUrl = btnImam.getAttribute('href');
                if (currentImamUrl) {
                    btnImam.setAttribute('href', currentImamUrl.split('?')[0] + '?year=' + selectedYear);
                }
            }
        });
    }

    // 4. Izin Create / Edit Day Counter Inputs
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    if (startDateInput && endDateInput) {
        const handleInput = function() {
            if (typeof window.updateDayCount === 'function') {
                window.updateDayCount();
            }
        };
        startDateInput.addEventListener('input', handleInput);
        endDateInput.addEventListener('input', handleInput);
        if (typeof window.updateDayCount === 'function') {
            window.updateDayCount();
        }
    }

    // 5. Gorevli / Gorevlendirme Career Fields Toggle and Selectors
    const sozlesmeTipSelect = document.getElementById('SozlesmeTipId');
    if (sozlesmeTipSelect) {
        sozlesmeTipSelect.addEventListener('change', function() {
            if (typeof window.toggleCareerFields === 'function') {
                window.toggleCareerFields();
            }
        });
        if (typeof window.toggleCareerFields === 'function') {
            window.toggleCareerFields();
        }
    }

    const isMerkezCheckbox = document.getElementById('IsMerkezPersoneli');
    if (isMerkezCheckbox) {
        isMerkezCheckbox.addEventListener('change', function() {
            if (typeof window.toggleMerkezGorevAlani === 'function') {
                window.toggleMerkezGorevAlani();
            }
        });
        if (typeof window.toggleMerkezGorevAlani === 'function') {
            window.toggleMerkezGorevAlani();
        }
    }

    // Gorevli Create Deduplication AJAX handling
    const createForm = document.getElementById('createForm');
    if (createForm) {
        createForm.addEventListener('submit', function (e) {
            if (createForm.dataset.duplicateChecked === 'true') return;
            e.preventDefault();

            const btn = document.getElementById('btnSubmit');
            if (btn) btn.disabled = true;

            const warningBanner = document.getElementById('duplicateWarningBanner');
            if (warningBanner) warningBanner.classList.add('d-none');

            const formData = {
                Ad: document.getElementById('Ad')?.value || '',
                Soyad: document.getElementById('Soyad')?.value || '',
                TCKimlikNo: document.getElementById('TCKimlikNo')?.value || '',
                Email: document.getElementById('Email')?.value || ''
            };

            $.ajax({
                type: 'POST',
                url: '/Gorevli/CheckDuplicate',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                success: function (res) {
                    if (res.isDuplicate) {
                        if (res.type === 'absolute') {
                            alert(res.message);
                            if (btn) btn.disabled = false;
                        } else {
                            if (warningBanner) warningBanner.classList.remove('d-none');
                            if (btn) {
                                btn.value = 'Eminim, Kaydet';
                                btn.disabled = false;
                            }
                            createForm.dataset.duplicateChecked = 'true';
                        }
                    } else {
                        createForm.dataset.duplicateChecked = 'true';
                        createForm.submit();
                    }
                },
                error: function () {
                    createForm.dataset.duplicateChecked = 'true';
                    createForm.submit();
                }
            });
        });
    }

    // Gorevli Edit - Assign Location AJAX Confirm Click
    const assignConfirmBtn = document.getElementById('assignLocationConfirmBtn');
    if (assignConfirmBtn) {
        assignConfirmBtn.addEventListener('click', function () {
            const form = $('#assignLocationForm');
            const data = {
                gorevliId: form.find('input[name="gorevliId"]').val(),
                newKurumId: form.find('#newKurumId').val()
            };
            $.ajax({
                url: '/Gorevli/AssignNewLocation',
                type: 'POST',
                data: data,
                success: function (res) {
                    const fb = $('#assignLocationFeedback');
                    if (res.success) {
                        fb.html('<div class="alert alert-success">' + res.message + '</div>');
                        setTimeout(function () { location.reload(); }, 1500);
                    } else {
                        fb.html('<div class="alert alert-danger">' + res.message + '</div>');
                    }
                },
                error: function () {
                    $('#assignLocationFeedback').html('<div class="alert alert-danger">Bir hata oluştu.</div>');
                }
            });
        });
    }

    // 6. Select2 Initialization
    if ($.fn.select2) {
        const staffSearch = $('#staffSearch');
        if (staffSearch.length > 0) {
            staffSearch.select2({
                ajax: {
                    url: '/Gorevli/SearchStaff',
                    dataType: 'json',
                    delay: 250,
                    data: function (params) { return { term: params.term }; },
                    processResults: function (data) { return { results: data }; },
                    cache: true
                },
                placeholder: 'İsim, soyad veya e-posta ile arayın...',
                minimumInputLength: 3,
                allowClear: true,
                width: '100%'
            });
        }
    }

    // ── Select2 Bootstrap Modal Focus Fix ──────────────────────────────────
    // Bootstrap Modal enforces focus internally and blocks Select2 dropdowns.
    // Override enforceFocus so the Select2 search input can receive focus.
    if (typeof $.fn.modal !== 'undefined' && $.fn.modal.Constructor) {
        $.fn.modal.Constructor.prototype.enforceFocus = function () {};
    }

    // Initialize modal-bound Select2 AFTER modal is fully shown (avoids focus lock)
    const newAssignmentModal = document.getElementById('newAssignmentModal');
    if (newAssignmentModal && $.fn.select2) {
        newAssignmentModal.addEventListener('shown.bs.modal', function () {
            const modalSelects = $('#quickGorevliId, #quickKurumId, #newAssignmentModal select[name="YerineGelecekGorevliId"]');
            if (modalSelects.length > 0 && !modalSelects.first().data('select2')) {
                modalSelects.select2({
                    dropdownParent: $(newAssignmentModal),
                    width: '100%',
                    language: {
                        noResults: function () { return 'Sonuç bulunamadı'; },
                        searching: function () { return 'Aranıyor...'; }
                    }
                });
            }
        });
    }

    const filterSelects = $('#collapseFilter select[name="GorevliId"], #collapseFilter select[name="KurumId"]');
    if ($.fn.select2 && filterSelects.length > 0) {
        filterSelects.select2({
            width: '100%'
        });
    }

    const standardAssignSelects = $('#KurumId, #GorevliId');
    if ($.fn.select2 && standardAssignSelects.length > 0) {
        standardAssignSelects.select2({ width: '100%', placeholder: 'Seçiniz...' });
    }

    // 7. Focus Gorevli Scroll & Collapse open
    const urlParams = new URLSearchParams(window.location.search);
    const focusGorevliId = urlParams.get('focusGorevliId');
    if (focusGorevliId) {
        setTimeout(() => {
            const heading = document.getElementById('heading-' + focusGorevliId);
            if (heading) {
                heading.scrollIntoView({ behavior: 'smooth', block: 'center' });
                const collapseEl = document.getElementById('collapse-' + focusGorevliId);
                if (collapseEl) {
                    const collapse = bootstrap.Collapse.getOrCreateInstance(collapseEl);
                    collapse.show();
                }
                heading.classList.add('highlight-row');
                setTimeout(() => {
                    heading.classList.remove('highlight-row');
                }, 2000);
            }
        }, 500);
    }

    // 8. Gorevlendirme modal quick-assignment AJAX submit
    const quickAssignSubmitBtn = document.getElementById('quickAssignmentSubmitBtn');
    const quickAssignForm = document.getElementById('quickAssignmentForm');
    const assignmentErrorBanner = document.getElementById('assignmentErrorBanner');
    const assignmentErrorText = document.getElementById('assignmentErrorText');

    if (quickAssignSubmitBtn && quickAssignForm) {
        quickAssignSubmitBtn.addEventListener('click', function () {
            if (!quickAssignForm.checkValidity()) {
                quickAssignForm.reportValidity();
                return;
            }

            const originalHtml = quickAssignSubmitBtn.innerHTML;
            quickAssignSubmitBtn.disabled = true;
            quickAssignSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Kaydediliyor...';
            if (assignmentErrorBanner) assignmentErrorBanner.classList.add('d-none');

            const formData = new FormData(quickAssignForm);

            fetch(quickAssignForm.action, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
            .then(async response => {
                quickAssignSubmitBtn.disabled = false;
                quickAssignSubmitBtn.innerHTML = originalHtml;

                const contentType = response.headers.get('content-type') || '';
                if (contentType.includes('application/json')) {
                    const data = await response.json();
                    if (data.success === false && assignmentErrorBanner && assignmentErrorText) {
                        assignmentErrorText.textContent = data.message || 'Bilinmeyen hata.';
                        assignmentErrorBanner.classList.remove('d-none');
                        assignmentErrorBanner.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                } else {
                    window.location.href = '/Gorevlendirme';
                }
            })
            .catch(error => {
                quickAssignSubmitBtn.disabled = false;
                quickAssignSubmitBtn.innerHTML = originalHtml;
                console.error('Assignment submit error:', error);
            });
        });
    }

    // Placements index deep-link metadata handler
    const metaEl = document.getElementById('dashboard-deeplink-meta');
    if (metaEl) {
        const autoOpen = metaEl.getAttribute('data-auto-open') === 'true';
        const targetKurumId = parseInt(metaEl.getAttribute('data-target-id'), 10);

        if (autoOpen && targetKurumId > 0) {
            const kurumSelect = document.getElementById('quickKurumId');
            if (kurumSelect) {
                const matchedOption = kurumSelect.querySelector(`option[value="${targetKurumId}"]`);
                if (matchedOption) {
                    kurumSelect.value = targetKurumId;
                    $(kurumSelect).trigger('change');
                }
            }

            const modalEl = document.getElementById('newAssignmentModal');
            if (modalEl) {
                const bsModal = new bootstrap.Modal(modalEl);
                bsModal.show();
            }
        }
    }

    // Export to Excel trigger
    const btnExportExcel = document.getElementById('btnExportExcel');
    if (btnExportExcel) {
        btnExportExcel.addEventListener('click', function(e) {
            e.preventDefault();
            const form = $(this).closest('form');
            const currentAction = form.attr('action');
            const currentMethod = form.attr('method');
            
            form.attr('action', '/Gorevlendirme/ExportFilteredExcel');
            form.attr('method', 'GET');
            form.submit();
            
            form.attr('action', currentAction || '');
            form.attr('method', currentMethod || 'GET');
        });
    }

    // Pagination Event Delegation in Placements Index
    document.querySelectorAll('.page-nav-link').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const pageVal = this.getAttribute('data-page');
            if (pageVal && typeof window.changePage === 'function') {
                window.changePage(parseInt(pageVal, 10));
            }
        });
    });

    // Note Addition Event Delegation in Placements Index
    document.addEventListener('submit', function (e) {
        const formEl = e.target.closest('.add-note-form');
        if (formEl) {
            e.preventDefault();
            const assignmentId = formEl.getAttribute('data-assignment-id');
            if (assignmentId && typeof window.addNote === 'function') {
                window.addNote(assignmentId);
            }
        }
    });

    // Note Deletion Event Delegation in Placements Index
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.delete-note-btn');
        if (btn) {
            e.preventDefault();
            const noteId = btn.getAttribute('data-note-id');
            const assignmentId = btn.getAttribute('data-assignment-id');
            if (noteId && assignmentId && typeof window.deleteNote === 'function') {
                window.deleteNote(noteId, assignmentId);
            }
        }
    });

    // Row selection checkboxes init for Gorevli Index
    if (typeof window.syncCheckboxesUI === 'function') {
        window.syncCheckboxesUI();
    }

    // ── Gorevlendirme page: checkbox + FAB wiring ──
    const isGorevlendirmePage = window.location.pathname.toLowerCase().includes('/gorevlendirme') &&
        !window.location.pathname.toLowerCase().includes('/create') &&
        !window.location.pathname.toLowerCase().includes('/edit') &&
        !window.location.pathname.toLowerCase().includes('/details') &&
        !window.location.pathname.toLowerCase().includes('/delete');

    if (isGorevlendirmePage) {
        // Select-All
        const selectAll = document.getElementById('selectAllGorevlendirme');
        if (selectAll) {
            selectAll.addEventListener('change', function () {
                document.querySelectorAll('.row-checkbox').forEach(cb => {
                    cb.checked = selectAll.checked;
                });
                grvl_syncUI();
            });
        }

        // Individual rows
        document.addEventListener('change', function (e) {
            if (e.target && e.target.classList.contains('row-checkbox')) {
                grvl_syncUI();
            }
        });

        grvl_syncUI();
    }
});

// ── GLOBAL FUNCTIONS (EXPOSED TO WINDOW TO BE ACCESSIBLE BY INLINE HTML ATTRIBUTES) ──

window.handleFileSelect = function(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const nameEl = document.getElementById('fileName');
        const previewEl = document.getElementById('filePreview');
        if (nameEl) nameEl.textContent = file.name + ' (' + (file.size / 1024).toFixed(1) + ' KB)';
        if (previewEl) previewEl.classList.remove('d-none');
    }
};

window.updateDayCount = function() {
    const startInput = document.getElementById('startDate');
    const endInput = document.getElementById('endDate');
    const totalDaysInput = document.getElementById('totalDays');
    if (!startInput || !endInput || !totalDaysInput) return;

    const start = new Date(startInput.value);
    const end   = new Date(endInput.value);
    if (!isNaN(start) && !isNaN(end) && end >= start) {
        const diffTime = Math.abs(end - start);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1; // inclusive
        totalDaysInput.value = diffDays;
    } else {
        totalDaysInput.value = '';
    }
};

window.toggleCareerFields = function() {
    var select = document.getElementById('SozlesmeTipId');
    if (!select) return;
    var selectedOption = select.options[select.selectedIndex];
    var isLocal = selectedOption && selectedOption.getAttribute('data-is-local') === 'true';
    var selectedText = selectedOption ? selectedOption.textContent.toLowerCase() : '';
    var isFrenchContract = isLocal ||
        selectedText.includes('yurtdı') ||
        selectedText.includes('dernek') ||
        selectedText.includes('yerel') ||
        selectedText.includes('cdi') ||
        selectedText.includes('cdd');

    var diyanetGiris = document.getElementById('diyanet-giris-container');
    var emeklilik = document.getElementById('emeklilik-container');
    var tcGroup = document.getElementById('tcKimlikGroup');
    var frGroup = document.getElementById('frenchIdGroup');

    if (isLocal) {
        if (diyanetGiris) diyanetGiris.style.display = 'none';
        if (emeklilik) diyanetGiris.style.display = 'none'; // wait, emeklilik diyanetGiris logic
        if (emeklilik) emeklilik.style.display = 'none';
    } else {
        if (diyanetGiris) diyanetGiris.style.display = '';
        if (emeklilik) emeklilik.style.display = '';
    }

    if (isFrenchContract) {
        if (tcGroup) { tcGroup.style.opacity = '0.45'; tcGroup.title = 'Yurtdışı/Dernek sözleşmelilerde TC Kimlik zorunlu değildir.'; }
        if (frGroup) { frGroup.style.display = ''; frGroup.style.opacity = '1'; }
    } else {
        if (tcGroup) { tcGroup.style.opacity = '1'; tcGroup.title = ''; }
        if (frGroup) { frGroup.style.display = 'none'; }
    }
};

window.toggleMerkezGorevAlani = function() {
    var isChecked = $('#IsMerkezPersoneli').is(':checked');
    if (isChecked) {
        $('#merkez-gorev-alani-container').show();
    } else {
        $('#merkez-gorev-alani-container').hide();
        $('#MerkezGorevAlani').val('');
    }
};

window.changeSortOrder = function(sortOrder) {
    var form = document.getElementById('filterForm');
    if (!form) return;
    var input = form.querySelector('input[name="sortOrder"]') || form.querySelector('input[name="SortOrder"]');
    if (!input) {
        input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'sortOrder';
        form.appendChild(input);
    }
    input.value = sortOrder;
    form.submit();
};

window.executeBulkGorevliDelete = function() {
    const targetIds = window.selectedEntityIds || [];
    if (targetIds.length === 0) {
        alert("Lütfen sol sütundaki kutucuklardan silinecek görevlileri seçiniz.");
        return;
    }
    if (!confirm("Seçilen görevlileri güvenli silme işlemine tabi tutmak istediğinize emin misiniz?")) return;

    const tokenField = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Gorevli/BulkSoftDelete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': tokenField
        },
        body: JSON.stringify(targetIds)
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) { 
            window.clearAllSelections();
            location.reload(); 
        } 
        else { alert("İşlem Başarısız: " + data.message); }
    })
    .catch(err => {
        console.error("API error:", err);
        alert("Sunucu bağlantısı kurulamadı.");
    });
};

window.submitQuickExport = function() {
    var selectedIds = window.selectedEntityIds || [];
    if (selectedIds.length === 0) {
        alert("Lütfen en az bir görevli seçin.");
        return;
    }

    var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Gorevli/BulkExportSelected', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(selectedIds)
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            var link = document.createElement('a');
            link.href = 'data:' + data.contentType + ';base64,' + data.fileContents;
            link.download = data.fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        } else {
            alert("Hata: " + data.message);
        }
    })
    .catch(err => {
        console.error(err);
        alert("Sunucu bağlantısı kurulamadı.");
    });
};

window.submitCustomExport = function() {
    var checkedColumns = document.querySelectorAll('#ep-col-grid input:checked');
    if (checkedColumns.length === 0) {
        alert('Lütfen en az bir sütun seçin.');
        return;
    }
    
    var form = document.getElementById('filterForm');
    if (!form) return;
    
    var originalAction = form.action;
    var originalMethod = form.method;
    form.action = '/Gorevli/CustomExport';
    form.method = 'POST';
    
    var tempElements = [];
    var selectedIds = window.selectedEntityIds || [];
    selectedIds.forEach(function(id) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'SelectedIds';
        input.value = id;
        form.appendChild(input);
        tempElements.push(input);
    });
    
    checkedColumns.forEach(function(box) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'columns';
        input.value = box.value;
        form.appendChild(input);
        tempElements.push(input);
    });
    
    form.submit();
    
    form.action = originalAction;
    form.method = originalMethod;
    tempElements.forEach(function(el) {
        form.removeChild(el);
    });
};

window.changePage = function(pageNum) {
    // Determine which filter form to use (Gorevli Index vs Gorevlendirme Index)
    var formGorevli = document.getElementById('filterForm');
    var formGorevlendirme = document.querySelector('#collapseFilter form');

    if (formGorevli) {
        var pageInput = document.createElement('input');
        pageInput.type = 'hidden';
        pageInput.name = 'PageNumber';
        pageInput.value = pageNum;
        formGorevli.appendChild(pageInput);
        
        var pageSizeSelect = document.querySelector('select[name="PageSize"]');
        if (pageSizeSelect) {
            var pageSizeInput = document.createElement('input');
            pageSizeInput.type = 'hidden';
            pageSizeInput.name = 'PageSize';
            pageSizeInput.value = pageSizeSelect.value;
            formGorevli.appendChild(pageSizeInput);
        }
        formGorevli.submit();
    } else if (formGorevlendirme) {
        var pageInput = document.createElement('input');
        pageInput.type = 'hidden';
        pageInput.name = 'page';
        pageInput.value = pageNum;
        formGorevlendirme.appendChild(pageInput);
        formGorevlendirme.submit();
    } else {
        window.location.href = '?page=' + pageNum + '&PageNumber=' + pageNum;
    }
};

window.changePageSize = function(size) {
    var form = document.getElementById('filterForm');
    if (form) {
        var pageSizeInput = document.createElement('input');
        pageSizeInput.type = 'hidden';
        pageSizeInput.name = 'PageSize';
        pageSizeInput.value = size;
        form.appendChild(pageSizeInput);

        var pageInput = document.createElement('input');
        pageInput.type = 'hidden';
        pageInput.name = 'PageNumber';
        pageInput.value = 1;
        form.appendChild(pageInput);

        form.submit();
    } else {
        window.location.href = '?PageSize=' + size + '&PageNumber=1';
    }
};

window.addNote = function(gorevliId) {
    const isGorevlendirme = window.location.pathname.toLowerCase().includes("gorevlendirme");
    const url = isGorevlendirme ? '/Gorevlendirme/AddNote' : '/Gorevli/AddNote';
    const form = document.getElementById(`note-form-${gorevliId}`);
    const textarea = document.getElementById(`note-input-${gorevliId}`);
    if (!form) return;
    const formData = new FormData(form);
    
    if (isGorevlendirme) {
        formData.append('gorevlendirmeId', gorevliId);
    } else {
        formData.append('gorevliId', gorevliId);
    }
    
    fetch(url, {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            if (textarea) textarea.value = '';
            location.reload();
        } else {
            alert('Not eklenirken hata oluştu: ' + (data.message || 'Bilinmeyen hata'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Bir hata oluştu.');
    });
};

window.deleteNote = function(notId, parentId) {
    if (!confirm('Bu notu silmek istediğinizden emin misiniz?')) return;
    const isGorevlendirme = window.location.pathname.toLowerCase().includes("gorevlendirme");
    const url = isGorevlendirme ? '/Gorevlendirme/DeleteNote' : '/Gorevli/DeleteNote';
    
    const formData = new FormData();
    formData.append('id', notId);
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) formData.append('__RequestVerificationToken', token);
    
    fetch(url, {
        method: 'POST',
        body: formData,
        headers: token ? { 'RequestVerificationToken': token } : {}
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            location.reload();
        } else {
            alert('Not silinirken hata oluştu: ' + (data.message || 'Bilinmeyen hata'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Bir hata oluştu.');
    });
};

window.editInlineNote = function(noteId) {
    const textEl = document.getElementById('note-text-' + noteId);
    const formEl = document.getElementById('note-edit-form-' + noteId);
    if (textEl) textEl.classList.add('d-none');
    if (formEl) formEl.classList.remove('d-none');
};

window.cancelInlineNoteEdit = function(noteId) {
    const textEl = document.getElementById('note-text-' + noteId);
    const formEl = document.getElementById('note-edit-form-' + noteId);
    if (textEl) textEl.classList.remove('d-none');
    if (formEl) formEl.classList.add('d-none');
};

window.saveInlineNote = function(noteId) {
    const textarea = document.getElementById('note-textarea-' + noteId);
    if (!textarea) return;
    var content = textarea.value;
    if (!content.trim()) {
        alert('Not içeriği boş olamaz.');
        return;
    }

    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Gorevli/EditNote',
        type: 'POST',
        data: {
            noteId: noteId,
            content: content,
            __RequestVerificationToken: token
        },
        success: function(res) {
            if (res.success) {
                const textEl = document.getElementById('note-text-' + noteId);
                if (textEl) textEl.textContent = content;
                window.cancelInlineNoteEdit(noteId);
            } else {
                alert('Hata: ' + res.message);
            }
        },
        error: function() {
            alert('Sunucuyla iletişim kurulamadı.');
        }
    });
};

window.deleteInlineNote = function(noteId) {
    if (!confirm('Bu notu silmek istediğinize emin misiniz?')) return;

    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Gorevli/DeleteNote',
        type: 'POST',
        data: {
            id: noteId,
            __RequestVerificationToken: token
        },
        success: function(res) {
            if (res.success) {
                var el = document.getElementById('note-container-' + noteId);
                if (el) el.remove();
            } else {
                alert('Hata: ' + res.message);
            }
        },
        error: function() {
            alert('Sunucuyla iletişim kurulamadı.');
        }
    });
};

// Checkbox helper functions for Gorevli Index
window.selectedEntityIds = [];

window.syncCheckboxesUI = function() {
    try {
        var stored = sessionStorage.getItem('selectedGorevliIds');
        if (stored) {
            window.selectedEntityIds = JSON.parse(stored);
        }
    } catch (e) {
        console.error(e);
    }

    var rowCheckboxes = document.querySelectorAll('.gorevli-select');
    var allChecked = rowCheckboxes.length > 0;
    var noneChecked = true;

    rowCheckboxes.forEach(function(cb) {
        var id = parseInt(cb.getAttribute('data-id'));
        var isChecked = window.selectedEntityIds.includes(id);
        cb.checked = isChecked;
        if (isChecked) {
            noneChecked = false;
        } else {
            allChecked = false;
        }
    });

    var selectAll = document.getElementById('selectAllGorevli');
    if (selectAll) {
        selectAll.checked = allChecked && rowCheckboxes.length > 0;
        selectAll.indeterminate = (!allChecked && !noneChecked);
    }

    var bar = document.getElementById('contextual-action-bar');
    var badge = document.getElementById('selected-badge');
    if (bar) {
        if (window.selectedEntityIds.length > 0) {
            bar.classList.add('show');
            if (badge) {
                badge.textContent = window.selectedEntityIds.length + ' Görevli Seçildi';
            }
        } else {
            bar.classList.remove('show');
        }
    }

    var rowsBadge = document.getElementById('ep-selected-rows-badge');
    var rowsCount = document.getElementById('ep-selected-rows-count');
    if (rowsBadge && rowsCount) {
        if (window.selectedEntityIds.length > 0) {
            rowsBadge.style.display = 'inline-flex';
            rowsCount.textContent = window.selectedEntityIds.length;
        } else {
            rowsBadge.style.display = 'none';
        }
    }
};

window.clearAllSelections = function() {
    window.selectedEntityIds = [];
    sessionStorage.setItem('selectedGorevliIds', JSON.stringify([]));
    window.syncCheckboxesUI();
};

window.toggleExportSection = function() {
    var content = document.getElementById('exportSectionContent');
    var chevron = document.querySelector('.ep-trigger .ep-chevron-icon i');
    if (!content) return;
    if (content.classList.contains('show')) {
        content.classList.remove('show');
        if (chevron) chevron.className = 'bi bi-chevron-down';
    } else {
        content.classList.add('show');
        if (chevron) chevron.className = 'bi bi-chevron-up';
        if (!window.colsLoaded) {
            window.fetchColumns();
        }
    }
};

window.fetchColumns = function() {
    var grid = document.getElementById('ep-col-grid');
    if (!grid) return;
    fetch('/Export/Columns?module=Gorevli')
        .then(r => r.json())
        .then(cols => {
            grid.innerHTML = '';
            cols.forEach(col => {
                var wrap = document.createElement('div');
                wrap.className = 'ep-checkbox-item' + (col.includeInQuickExport ? ' active' : '');
                
                var cb = document.createElement('input');
                cb.type = 'checkbox';
                cb.id = 'ep-col-' + col.propertyName;
                cb.value = col.propertyName;
                cb.checked = col.includeInQuickExport;
                cb.addEventListener('change', function() {
                    wrap.classList.toggle('active', cb.checked);
                    window.updateSelectedColumnsCount();
                });
                
                var lbl = document.createElement('label');
                lbl.htmlFor = cb.id;
                lbl.textContent = col.displayName;
                lbl.title = col.displayName;
                
                wrap.appendChild(cb);
                wrap.appendChild(lbl);
                grid.appendChild(wrap);
            });
            window.colsLoaded = true;
            window.updateSelectedColumnsCount();
        })
        .catch(err => {
            grid.innerHTML = '<div class="text-danger small"><i class="bi bi-exclamation-triangle-fill me-1"></i>Sütunlar yüklenemedi.</div>';
        });
};

window.updateSelectedColumnsCount = function() {
    var count = document.querySelectorAll('#ep-col-grid input:checked').length;
    const countEl = document.getElementById('ep-selected-cols-count');
    if (countEl) countEl.textContent = count + ' sütun seçili';
};

window.toggleAllColumns = function() {
    var boxes = document.querySelectorAll('#ep-col-grid input[type="checkbox"]');
    var allChecked = true;
    boxes.forEach(b => { if (!b.checked) allChecked = false; });
    boxes.forEach(b => {
        b.checked = !allChecked;
        var wrap = b.closest('.ep-checkbox-item');
        if (wrap) wrap.classList.toggle('active', !allChecked);
    });
    window.updateSelectedColumnsCount();
};

window.toggleExportPanelFromFloatingBar = function() {
    var content = document.getElementById('exportSectionContent');
    if (content) {
        if (!content.classList.contains('show')) {
            window.toggleExportSection();
        }
        content.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};

window.triggerBulkExcelQuick = function() {
    window.submitQuickExport();
};

window.triggerBulkDelete = function() {
    var ids = window.selectedEntityIds || [];
    if (ids.length === 0) return;
    if (!confirm(ids.length + ' adet seçilen görevliyi sistemden silmek istediğinize emin misiniz?')) return;

    var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/Gorevli/BulkDelete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(ids)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('✓ ' + data.count + ' görevli silindi!');
            window.clearAllSelections();
            location.reload();
        } else {
            alert('✗ Hata: ' + (data.message || 'Silme işlemi gerçekleştirilemedi.'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('✗ Sunucu bağlantısı başarısız.');
    });
};

// ══════════════════════════════════════════════════════════════════
// GOREVLENDIRME MODULE — Checkbox, FAB & Bulk Operation Functions
// ══════════════════════════════════════════════════════════════════

window._grvlSelectedIds = [];

window.grvl_syncUI = function () {
    const checkboxes = document.querySelectorAll('.row-checkbox');
    window._grvlSelectedIds = [];
    checkboxes.forEach(cb => {
        if (cb.checked) {
            const id = parseInt(cb.getAttribute('data-id'), 10);
            if (id > 0) window._grvlSelectedIds.push(id);
        }
    });

    // Update select-all state
    const selectAll = document.getElementById('selectAllGorevlendirme');
    if (selectAll && checkboxes.length > 0) {
        const checkedCount = window._grvlSelectedIds.length;
        selectAll.checked = checkedCount === checkboxes.length;
        selectAll.indeterminate = checkedCount > 0 && checkedCount < checkboxes.length;
    }

    // FAB visibility
    const bar = document.getElementById('grvl-action-bar');
    const badge = document.getElementById('grvl-selected-badge');
    if (bar) {
        if (window._grvlSelectedIds.length > 0) {
            bar.classList.add('show');
            if (badge) badge.textContent = window._grvlSelectedIds.length + ' Seçili';
        } else {
            bar.classList.remove('show');
        }
    }
};

window.grvl_clearAll = function () {
    document.querySelectorAll('.row-checkbox').forEach(cb => { cb.checked = false; });
    const selectAll = document.getElementById('selectAllGorevlendirme');
    if (selectAll) { selectAll.checked = false; selectAll.indeterminate = false; }
    window._grvlSelectedIds = [];
    const bar = document.getElementById('grvl-action-bar');
    if (bar) bar.classList.remove('show');
};

window.grvl_quickExcel = function () {
    const ids = window._grvlSelectedIds || [];
    if (ids.length === 0) { alert('Lütfen en az bir kayıt seçin.'); return; }
    window.location.href = '/Gorevlendirme/ExportSelectedExcel?ids=' + ids.join(',');
};

window.grvl_columnExcel = function () {
    const ids = window._grvlSelectedIds || [];
    if (ids.length === 0) { alert('Lütfen en az bir kayıt seçin.'); return; }

    const selectedCols = Array.from(
        document.querySelectorAll('.grvl-col-check:checked')
    ).map(cb => cb.value);

    if (selectedCols.length === 0) { alert('Lütfen en az bir sütun seçin.'); return; }

    const url = '/Gorevlendirme/ExportSelectedExcel?ids=' + ids.join(',') +
                '&columns=' + selectedCols.join(',');

    // Close modal then navigate
    const modalEl = document.getElementById('grvlColModal');
    if (modalEl) {
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }
    window.location.href = url;
};

window.grvl_bulkDelete = function () {
    const ids = window._grvlSelectedIds || [];
    if (ids.length === 0) { alert('Lütfen silinecek kayıtları seçin.'); return; }

    if (!confirm(ids.length + ' görevlendirmeyi güvenli silmek istediğinize emin misiniz?\n(Kayıtlar veritabanından fiziksel olarak silinmez, IsDeleted işaretlenir.)')) return;

    fetch('/Gorevlendirme/BulkSoftDelete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(ids)
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            window.grvl_clearAll();
            location.reload();
        } else {
            alert('İşlem başarısız: ' + (data.message || 'Bilinmeyen hata.'));
        }
    })
    .catch(err => {
        console.error('BulkSoftDelete error:', err);
        alert('Sunucu bağlantısı kurulamadı.');
    });
};

// ── İzin Create — Görevli telefonu otomatik doldurma ──────────────────────────
(function initIzinContactFill() {
    const gorevliSelect = document.getElementById('izinGorevliSelect');
    const phoneInput    = document.getElementById('izinIrtibatTel');
    if (!gorevliSelect || !phoneInput) return;

    gorevliSelect.addEventListener('change', function () {
        const gorevliId = this.value;
        if (!gorevliId) {
            phoneInput.value = '';
            return;
        }
        fetch('/Gorevli/GetContactInfo?id=' + encodeURIComponent(gorevliId))
            .then(r => r.json())
            .then(data => {
                if (data && data.phone) {
                    phoneInput.value = data.phone;
                }
            })
            .catch(err => console.warn('Contact info fetch failed:', err));
    });

    // Pre-fill if page loads with a preselected gorevli (e.g. back-navigation)
    if (gorevliSelect.value) {
        gorevliSelect.dispatchEvent(new Event('change'));
    }
})();
