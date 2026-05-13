class TableHighlighter {
    constructor(inputSelector, containerSelector) {
        this.input = document.querySelector(inputSelector);
        this.container = document.querySelector(containerSelector);
        if (this.input && this.container) {
            this.input.addEventListener('input', () => this.highlight(this.input.value));
        }
    }

    highlight(term) {
        this.clear();
        const trimmedTerm = term.trim();
        if (!trimmedTerm) return;
        const regex = new RegExp(`(${this.escapeRegExp(trimmedTerm)})`, 'gi');

        const tbody = this.container.querySelector('tbody');
        if (tbody) {
            this.walkAndHighlight(tbody, regex);
        }
    }

    walkAndHighlight(node, regex) {
        if (node.nodeType === 1) {
            const tag = node.nodeName.toUpperCase();
            if (['BUTTON', 'A', 'SCRIPT', 'STYLE', 'MARK', 'SVG', 'PATH', 'FORM', 'INPUT', 'SELECT'].includes(tag)) return;
            if (node.classList && node.classList.contains('no-highlight')) return;
        }

        if (node.nodeType === 3) {
            const match = node.nodeValue.match(regex);
            if (match && node.nodeValue.trim() !== '') {
                const span = document.createElement('span');
                span.innerHTML = node.nodeValue.replace(regex, '<mark class="search-highlight">$1</mark>');
                node.parentNode.replaceChild(span, node);
            }
        } else if (node.nodeType === 1) {
            Array.from(node.childNodes).forEach(child => this.walkAndHighlight(child, regex));
        }
    }

    clear() {
        const tbody = this.container.querySelector('tbody');
        if (!tbody) return;

        const marks = tbody.querySelectorAll('mark.search-highlight');
        marks.forEach(mark => {
            const parent = mark.parentNode;
            parent.replaceChild(document.createTextNode(mark.textContent), mark);
            parent.normalize();
        });
        const spans = tbody.querySelectorAll('span');
        spans.forEach(span => {
            if (span.childNodes.length === 1 && span.childNodes[0].nodeType === 3 && !span.attributes.length) {
                const parent = span.parentNode;
                parent.replaceChild(document.createTextNode(span.textContent), span);
                parent.normalize();
            }
        });
    }

    escapeRegExp(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
}