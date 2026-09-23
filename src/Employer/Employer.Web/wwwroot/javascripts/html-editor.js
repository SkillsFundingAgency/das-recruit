/*
    Html editor source
      - uses tiptap headless editor, see https://github.com/ueberdosis/tiptap
      - customises the editor to improve accessibility and control formatting 
*/
import { Editor, Extension } from '@tiptap/core'
import BulletList from '@tiptap/extension-bullet-list'
import ListItem from '@tiptap/extension-list-item'
import Document from '@tiptap/extension-document'
import Text from '@tiptap/extension-text'
import Paragraph from '@tiptap/extension-paragraph'
import ListKeymap from '@tiptap/extension-list-keymap'
import { UndoRedo } from '@tiptap/extensions'

/*
    Extend BulletList to:
        - remove the Control+Shift+8 shortcut, it could interfere with screen reader shortcuts - achieved by not declaring the shortcut which the base control declares
*/
const CustomBulletList = BulletList.extend({
    addKeyboardShortcuts() { return {} },
})

/*
    Extend ListItem to:
        - avoid the 'tab trap' to improve accessibility - achieved by not declaring the tab key shortcuts the base control declares
        - allow the 'Enter' key to split a list item and create a new item on the new line 
 */
// tab should exit the control
const CustomListItem = ListItem.extend({
    addKeyboardShortcuts() {
        return {
            Enter: () => this.editor.commands.splitListItem(this.name),
        }
    },
})

/*
    Create an extension to:
        - remove pasting of html into the control, there's too many edge cases to handle
*/
const CleanStylesExtension = Extension.create({
    name: 'cleanStyles',
    transformPastedHTML(html)  { return html.replace(/<(?:"[^"]*"['"]*|'[^']*'['"]*|[^'">])+>/g, '') }
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
        'click': (editor, editorActions) => (e) => {
            editorActions.toggleBulletList(editor)
            e.preventDefault()
        }
    }
}

const copyAttr = ['aria-label', 'aria-labelledby', 'aria-describedby', 'aria-required', 'required']
const attrMap = { 'required': 'aria-required' }

/*
    Copy aria attributes if specified
    Also maps attributes into aria ones as per the attrMap above, this is because the editor is not a standard semantic control
 */
function copyAriaAttributes(attrs, el) {
    copyAttr.reduce((acc, val) => {
        if (el.hasAttribute(val)) {
            let attrVal = el.getAttribute(val)
            attrVal = attrVal === '' || attrVal === undefined || attrVal === null ? 'true' : attrVal
            acc[attrMap[val] ?? val] = attrVal
        }
        return acc
    }, attrs)
}

/*
    Factory method to create custom keyboard shortcuts to control the toolbar behaviour
*/
const CreateCustomKeyboardShortcuts = (focusButton, editorActions) => Extension.create({
    name: 'customShortcuts',
    addKeyboardShortcuts() {
        return {
            'Alt-F10': () => {
                focusButton.focus()
                return true
            },
            'Mod-Shift-8': () => {
                editorActions.toggleBulletList(this.editor)
                return true
            },
        }
    }
})

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
    //  - need to implement toolbar keyboard navigation for accessibility
    //  - set this: toolbar.setAttribute('role', 'toolbar')
    //  - move aria-controls to the toolbar: toolbar.setAttribute('aria-controls', id)
    //  - remove aria-controls from individual buttons
    //  - add tab index from 0 to buttons

    bulletListBtn.setAttribute('aria-controls', id)
    bulletListBtn.setAttribute('aria-keyshortcuts', 'Control+Shift+8 Meta+Shift+8')
    toolbar.classList.add('html-editor-toolbar')
    toolbar.setAttribute('aria-keyshortcuts', 'Alt+F10')

    target.insertAdjacentElement("afterend", container)
    container.appendChild(toolbar)
    toolbar.appendChild(bulletListBtn)

    return { toolbar, container, bulletListBtn }
}

function initHtmlEditor(el) {
    hideTargetControl(el)
    const targetId = el.getAttribute('id')
    const id = crypto.randomUUID()
    const { toolbar, container, bulletListBtn } = createToolbar(el, id, targetId)
    const announceArea = el.hasAttribute('data-action-announce-id')
        ? document.getElementById(el.getAttribute('data-action-announce-id'))
        : undefined
    
    const editorActions = actionsFactory(announceArea)
    const CustomKeyboardShortcuts = CreateCustomKeyboardShortcuts(bulletListBtn, editorActions)

    // Default attributes for the editor
    let attrs = {
        id: id,
        class: 'html-editor-textarea govuk-textarea',
        role: 'textbox',
        'aria-multiline': 'true',
        'aria-readonly': 'false',
    };

    copyAriaAttributes(attrs, el)
    const selectionUpdate = selectionUpdateFactory(bulletListBtn)

    // Create the editor
    const editor = new Editor({
        element: container,
        extensions: [
            UndoRedo,
            Document,
            Text,
            Paragraph,
            CustomBulletList,
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
            selectionUpdate(editor)
        },
        onCreate({ editor }) {
            selectionUpdate(editor)
        }
    })

    bulletListBtn.addEventListener('click', buttons['bullet-list']['click'](editor, editorActions))
    return editor;
}

const selectionUpdateFactory = (bulletListBtn) => (editor) => {
    if (editor.isActive('bulletList')) {
        bulletListBtn.classList.add('active')
        bulletListBtn.setAttribute('aria-pressed', 'true')
    } else {
        bulletListBtn.classList.remove('active')
        bulletListBtn.setAttribute('aria-pressed', 'false')
    }
}

const announceTexts = {
    'bulletList': {
        true: 'Bullet list on',
        false: 'Bullet list off'
    }
}
const announceActionFactory = (announceElement) => (editor, action) => {
    if (!announceElement) return;
    announceElement.innerText = announceTexts[action][editor.isActive(action)]
}

const actionsFactory = (announceElement) => {
    const actionAnnouncer = announceActionFactory(announceElement)
    return {
        toggleBulletList: (editor) => {
            editor.commands.toggleBulletList()
            actionAnnouncer(editor, 'bulletList')
        }
    }
}

export { initHtmlEditor }