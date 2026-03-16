let _clinicianDropdownTargetId = 'referrerCode';

function SetClinicianTarget(targetId) {
    _clinicianDropdownTargetId = targetId;
}

async function SearchClinicians() {
    const firstName = document.getElementById('inlineFormInputName2').value;
    const lastName = document.getElementById('inlineFormInputName1').value;
    const isOnlyCurrent = document.getElementById('chkOnlyCurrent').checked;
    const isOnlyGP = document.getElementById('chkOnlyGP').checked;
    const isOnlyNonGP = document.getElementById('chkOnlyNonGP').checked;

    const url = `/Referral/SearchExternalClinicians?firstName=${encodeURIComponent(firstName)}&lastName=${encodeURIComponent(lastName)}&isOnlyCurrent=${isOnlyCurrent}&isOnlyGP=${isOnlyGP}&isOnlyNonGP=${isOnlyNonGP}`;

    try {
        const response = await fetch(url);
        const data = await response.json();

        const tbody = document.getElementById('tblClinicianResultsBody');
        tbody.innerHTML = '';

        if (data && data.length > 0) {
            data.forEach(item => {
                const fullName = `${item.title || ''} ${item.firstName || ''} ${item.lastName || ''}`.trim();
                const tr = document.createElement('tr');

                tr.innerHTML = `
                    <td>${item.masterClinicianCode}</td>
                    <td>${fullName}</td>
                    <td>${item.facility || ''}</td>
                    <td>${item.position || ''}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-primary"
                            onclick="SelectExternalClinician('${item.masterClinicianCode}', '${fullName.replace(/'/g, "\\'")}', '${(item.facility || '').replace(/'/g, "\\'")}')">
                            Select
                        </button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
            document.getElementById('tblClinicianResults').style.display = 'table';
        } else {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No clinicians found matching those criteria.</td></tr>';
            document.getElementById('tblClinicianResults').style.display = 'table';
        }
    } catch (error) {
        console.error('Error searching for clinicians:', error);
        alert('An error occurred whilst searching. Please try again.');
    }
}

function SelectExternalClinician(code, name, facility) {
    var referrerList = document.getElementById(_clinicianDropdownTargetId);
    var displayText = `${code} - ${name}, ${facility}`;

    if (referrerList && referrerList.choices) {
        referrerList.choices.setChoices([
            { value: code, label: displayText, selected: true }
        ], 'value', 'label', false);
    } else if (referrerList) {
        var optionExists = Array.from(referrerList.options).some(opt => opt.value === code);
        if (!optionExists) {
            var newOption = document.createElement("option");
            newOption.value = code;
            newOption.text = displayText;
            referrerList.add(newOption);
        }
        referrerList.value = code;
    }

    var modalEl = document.getElementById('centeredModalSuccess');
    var modalInstance = bootstrap.Modal.getInstance(modalEl);
    if (modalInstance) {
        modalInstance.hide();
    }
}

function selectGen() {
    var referrerList = document.getElementById(_clinicianDropdownTargetId);

    if (!referrerList) {
        console.error("Dropdown element not found: " + _clinicianDropdownTargetId);
        return;
    }

    var genOption = Array.from(referrerList.options).find(opt => opt.value === "GEN");

    if (genOption) {
        if (referrerList.choices) {
            referrerList.choices.setChoiceByValue("GEN");
        } else {
            referrerList.value = "GEN";
        }
    } else {
        alert("GEN option not found in the referrer list.");
    }
}