import { Editor, Extension } from 'https://esm.sh/@tiptap/core'
import BulletList from 'https://esm.sh/@tiptap/extension-bullet-list'
import ListItem from 'https://esm.sh/@tiptap/extension-list-item'
import Document from 'https://esm.sh/@tiptap/extension-document'
import Text from 'https://esm.sh/@tiptap/extension-text'
import Paragraph from 'https://esm.sh/@tiptap/extension-paragraph'
import ListKeymap from 'https://esm.sh/@tiptap/extension-list-keymap'
import { CharacterCount, UndoRedo } from 'https://esm.sh/@tiptap/extensions'

const CustomListItem = ListItem.extend({
    addKeyboardShortcuts() {
        return {
            Enter: () => this.editor.commands.splitListItem(this.name),
        }
    },
})

function hideTargetControl(target) {
    // hide the existing element
    target.classList.add("govuk-visually-hidden")
    target.setAttribute("tabindex", -1)
    target.setAttribute("aria-hidden", true)
}

function createToolbar(target) {
    // create our UI elements

    const container = document.createElement('div')
    const toolbar = document.createElement('div')
    const bulletListBtn = document.createElement('button')
    const span = document.createElement('span')
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg")
    const path = document.createElementNS("http://www.w3.org/2000/svg", "path")

    bulletListBtn.setAttribute('aria-pressed', 'false')
    bulletListBtn.setAttribute('aria-label', 'Bullet list')
    
    svg.setAttribute('xmlns', 'http://www.w3.org/2000/svg')
    svg.setAttribute('focusable', 'false')
    svg.setAttribute('width', '24')
    svg.setAttribute('height', '24')
    path.setAttribute('d', 'M11 5h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2Zm0 6h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2Zm0 6h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2ZM4.5 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Zm0 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Zm0 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Z')
    path.setAttribute('fill-rule', 'evenodd')
    
    svg.appendChild(path)
    
    toolbar.classList.add('html-editor-toolbar')
    toolbar.setAttribute('role', 'toolbar')
    
    target.insertAdjacentElement("afterend", toolbar)
    toolbar.insertAdjacentElement("afterend", container)
    toolbar.appendChild(bulletListBtn)
    span.appendChild(svg)
    bulletListBtn.appendChild(span)
    
    return { toolbar, container, btn: bulletListBtn }
}

function initHtmlEditor(el) {
    hideTargetControl(el)
    
    const { toolbar, container, btn } = createToolbar(el)

    // Custom keyboard shortcut to focus _our_ toolbar
    const CustomKeyboardShortcuts = Extension.create({
        name: 'customShortcuts',
        addKeyboardShortcuts() {
            return {
                'Alt-F10': () => {
                    btn.focus()
                    return true
                }
            }
        }
    })

    // Default attributes for the editor
    let attrs = {
        class: 'html-editor-textarea govuk-textarea',
        role: 'textbox',
        'aria-multiline': 'true',
        'aria-readonly': 'false',
    };

    // Copy aria attributes if specified
    ['aria-label', 'aria-labelledby', 'aria-describedby', 'aria-required'].reduce((acc, val) => {
        if (el.hasAttribute(val)) {
            acc[val] = el.getAttribute(val)
        }
        return acc
    }, attrs)

    // Create the editor
    const editor = new Editor({
        element: container,
        extensions: [
            CharacterCount,
            UndoRedo,
            Document,
            Text,
            Paragraph,
            BulletList,
            CustomListItem,
            ListKeymap,
            CustomKeyboardShortcuts
        ],
        content: el.value,
        injectCSS: true,
        editorProps: {
            attributes: attrs,
            transformPastedHTML(html) {
                return html.replace(/<(?:"[^"]*"['"]*|'[^']*'['"]*|[^'">])+>/g, '');
            }
        },
        onUpdate({ editor }) {
            el.value = editor.getHTML()
            el.dispatchEvent(new Event('input'))
            
        },
        onSelectionUpdate({ editor }) {
            if (editor.isActive('bulletList')) {
                btn.classList.add('active')
                btn.setAttribute('aria-pressed', 'true')
            } else {
                btn.classList.remove('active')
                btn.setAttribute('aria-pressed', 'false')
            }
        }
    })

    btn.addEventListener('click', (e) => {
        e.preventDefault()
        editor.commands.toggleBulletList()
    })

    return editor;
}

export { initHtmlEditor }