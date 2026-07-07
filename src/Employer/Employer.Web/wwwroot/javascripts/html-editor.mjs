import { Editor, Extension } from 'https://esm.sh/@tiptap/core'
import BulletList from 'https://esm.sh/@tiptap/extension-bullet-list'
import ListItem from 'https://esm.sh/@tiptap/extension-list-item'
import Document from 'https://esm.sh/@tiptap/extension-document'
import Text from 'https://esm.sh/@tiptap/extension-text'
import Paragraph from 'https://esm.sh/@tiptap/extension-paragraph'
import ListKeymap from 'https://esm.sh/@tiptap/extension-list-keymap'
import { CharacterCount, UndoRedo } from 'https://esm.sh/@tiptap/extensions'

// create our own list item type to avoid the unwanted tab/shift+tab shortcuts
// the default implementation has to create nested lists, we just want those
// shortcuts to tab out of the control
const CustomListItem = ListItem.extend({
    addKeyboardShortcuts() {
        return {
            Enter: () => this.editor.commands.splitListItem(this.name),
        }
    },
})

const CleanStylesExtension = Extension.create({
    name: 'cleanStyles',
    transformPastedHTML(html) {
        if (!this.editor.isActive('bulletList'))
        {
            return html
        }
        
        // if we're within a list already, strip out additional <ul> or <p> tags
        // we do this to try and avoid formatting we don't want e.g. nested lists
        return html.replace(/<\/?(?:ul|p)+>/g, '')
    }
})

function hideTargetControl(target) {
    // hide the existing element
    target.classList.add("govuk-visually-hidden")
    target.setAttribute("tabindex", -1)
    target.setAttribute("aria-hidden", true)
}

const buttons = {
    'bullet-list' : {
        'aria-label': 'Bullet list',
        'icon-path': 'M11 5h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2Zm0 6h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2Zm0 6h8c.6 0 1 .4 1 1s-.4 1-1 1h-8a1 1 0 0 1 0-2ZM4.5 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Zm0 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Zm0 6c0-.4.1-.8.4-1 .3-.4.7-.5 1.1-.5.4 0 .8.1 1 .4.4.3.5.7.5 1.1 0 .4-.1.8-.4 1-.3.4-.7.5-1.1.5-.4 0-.8-.1-1-.4-.4-.3-.5-.7-.5-1.1Z',
        'click': (editor) => (e) => {
            editor.commands.toggleBulletList()
            e.preventDefault()
        }
    }
}

const copyAttr = ['aria-label', 'aria-labelledby', 'aria-describedby', 'aria-required', 'required']
const attrMap = { 'required': 'aria-required' }

function copyAriaAttributes(attrs, el) {
    // Copy aria attributes if specified
    // Also maps attributes into aria ones as per the attrMap above,
    // this is because the editor is not a standard semantic control
    copyAttr.reduce((acc, val) => {
        if (el.hasAttribute(val)) {
            let attrVal = el.getAttribute(val)
            attrVal = attrVal === '' || attrVal === undefined || attrVal === null ? 'true' : attrVal
            acc[attrMap[val] ?? val] = attrVal
        }
        return acc
    }, attrs)
}

function createToolbarBtn(name, ariaLabelName) {
    if (!buttons.hasOwnProperty(name)) {
        return
    }

    const btn = document.createElement('button')
    const span = document.createElement('span')
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg")
    const path = document.createElementNS("http://www.w3.org/2000/svg", "path")

    btn.setAttribute('aria-pressed', 'false')
    
    if (ariaLabelName) {
        btn.setAttribute('aria-label', `${buttons[name]['aria-label']} for ${ariaLabelName}`)
    } else {
        btn.setAttribute('aria-label', buttons[name]['aria-label'])
    }
    btn.classList.add('govuk-button')
    btn.classList.add('govuk-button--secondary')

    svg.setAttribute('xmlns', 'http://www.w3.org/2000/svg')
    svg.setAttribute('focusable', 'false')
    svg.setAttribute('width', '24')
    svg.setAttribute('height', '24')

    path.setAttribute('d', buttons[name]['icon-path'])
    path.setAttribute('fill-rule', 'evenodd')

    svg.appendChild(path)
    span.appendChild(svg)
    btn.appendChild(span)
    
    return btn
}

function createToolbar(target, id, targetId) {

    let label = undefined
    if (targetId) {
        const labelCtl = document.getElementById(`${targetId}-Label`)
        if (labelCtl) {
            label = labelCtl.innerText.trim()
        }
    }
    
    const container = document.createElement('div')
    const toolbar = document.createElement('div')
    const bulletListBtn = createToolbarBtn('bullet-list', label)

    // if we have 3 or more buttons, then:
    //  - need to implement toolbar keyboard navigation for accessbility
    //  - set this: toolbar.setAttribute('role', 'toolbar')
    //  - move aria-controls to the toolbar: toolbar.setAttribute('aria-controls', id)
    //  - remove aria-controls from individual buttons
    //  - add tab index from 0 to buttons

    bulletListBtn.setAttribute('aria-controls', id)
    toolbar.classList.add('html-editor-toolbar')
    
    target.insertAdjacentElement("afterend", toolbar)
    toolbar.insertAdjacentElement("afterend", container)
    toolbar.appendChild(bulletListBtn)
    
    return { toolbar, container, bulletListBtn }
}

function initHtmlEditor(el) {
    hideTargetControl(el)
    const targetId = el.getAttribute('id')

    const id = crypto.randomUUID()
    const { toolbar, container, bulletListBtn } = createToolbar(el, id, targetId)

    // Custom keyboard shortcut to focus this instance's toolbar
    const CustomKeyboardShortcuts = Extension.create({
        name: 'customShortcuts',
        addKeyboardShortcuts() {
            return {
                'Alt-F10': () => {
                    bulletListBtn.focus()
                    return true
                }
            }
        }
    })

    // Default attributes for the editor
    let attrs = {
        id: id,
        class: 'html-editor-textarea govuk-textarea',
        role: 'textbox',
        'aria-multiline': 'true',
        'aria-readonly': 'false',
    };

    copyAriaAttributes(attrs, el)

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
            CustomKeyboardShortcuts,
            CleanStylesExtension
        ],
        content: el.value,
        injectCSS: true,
        editorProps: {
            attributes: attrs,
        },
        onUpdate({ editor }) {
            const html = editor.getHTML()
            el.value = html === '<p></p>'
                ? null
                : html
                
            el.dispatchEvent(new Event('input'))
        },
        onSelectionUpdate({ editor }) {
            if (editor.isActive('bulletList')) {
                bulletListBtn.classList.add('active')
                bulletListBtn.setAttribute('aria-pressed', 'true')
            } else {
                bulletListBtn.classList.remove('active')
                bulletListBtn.setAttribute('aria-pressed', 'false')
            }
        }
    })

    bulletListBtn.addEventListener('click', buttons['bullet-list']['click'](editor))
    return editor;
}

export { initHtmlEditor }