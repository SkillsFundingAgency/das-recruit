// this is the import file used during the build process to generate the tiptap.min.js file
// this file should not be included in the main net build
import { Editor, Extension } from '@tiptap/core'
import BulletList from '@tiptap/extension-bullet-list'
import ListItem from '@tiptap/extension-list-item'
import Document from '@tiptap/extension-document'
import Text from '@tiptap/extension-text'
import Paragraph from '@tiptap/extension-paragraph'
import ListKeymap from '@tiptap/extension-list-keymap'
import { CharacterCount, UndoRedo } from '@tiptap/extensions'

export { Editor, Extension, CharacterCount, UndoRedo, BulletList, ListItem, Document, Text, Paragraph, ListKeymap }