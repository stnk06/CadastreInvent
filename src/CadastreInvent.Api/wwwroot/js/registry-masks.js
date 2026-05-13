class RegistryMaskManager {
    init() {
        this.initSpatialUnitMasks();
        this.initPartyGovRegMasks();
        this.initPartyContactMasks();
    }
    initSpatialUnitMasks() {
        const el = document.getElementById('reg-cad-number');
        if (el && typeof Inputmask !== 'undefined') {
            Inputmask({
                regex: "^\\d{2}:\\d{2}:\\d{6,7}:\\d{1,10}$",
                placeholder: "_",
                showMaskOnHover: false,
                showMaskOnFocus: true,
                clearIncomplete: true,
                removeMaskOnSubmit: true
            }).mask(el);
        }
    }
    initPartyGovRegMasks() {
        const selectEl = document.getElementById('gov-reg-type');
        const inputEl = document.getElementById('gov-reg-input');
        if (selectEl && inputEl && typeof Inputmask !== 'undefined') {
            const applyMask = () => {
                if (inputEl.inputmask) inputEl.inputmask.remove();
                inputEl.value = '';
                let maskConfig = {
                    clearIncomplete: true,
                    showMaskOnHover: false,
                    removeMaskOnSubmit: true
                };
                switch (selectEl.value) {
                    case "СНИЛС": maskConfig.mask = "999-999-999 99"; break;
                    case "ИНН_ФЛ": maskConfig.mask = "999999999999"; break;
                    case "ИНН_ЮЛ": maskConfig.mask = "9999999999"; break;
                    case "ОГРН": maskConfig.mask = "9999999999999"; break;
                    case "ОГРНИП": maskConfig.mask = "999999999999999"; break;
                    default: maskConfig.regex = "^\\d+$";
                }
                Inputmask(maskConfig).mask(inputEl);
            };
            selectEl.addEventListener("change", applyMask);
            applyMask();
        }
    }
    initPartyContactMasks() {
        const selectEl = document.getElementById('contact-type');
        const inputEl = document.getElementById('contact-input');
        if (selectEl && inputEl && typeof Inputmask !== 'undefined') {
            const applyMask = () => {
                if (inputEl.inputmask) inputEl.inputmask.remove();
                inputEl.value = '';
                if (selectEl.value === "Телефон") {
                    Inputmask({
                        mask: "+7 (999) 999-99-99",
                        clearIncomplete: true,
                        showMaskOnHover: false,
                        removeMaskOnSubmit: true
                    }).mask(inputEl);
                } else {
                    Inputmask({
                        regex: "^[А-Яа-яЁё0-9\\s\\.,\\-\\/]{5,250}$",
                        showMaskOnHover: false,
                        removeMaskOnSubmit: false
                    }).mask(inputEl);
                }
            };
            selectEl.addEventListener("change", applyMask);
            applyMask();
        }
    }
}
document.addEventListener("DOMContentLoaded", () => {
    new RegistryMaskManager().init();
});