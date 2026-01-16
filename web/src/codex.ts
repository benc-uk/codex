// ===================================================================================
// Codex - Interactive Storytelling Platform
// Copyright (C) 2025 Ben Coleman, Licensed under the MIT License
// codex.ts - Main web application entry point
// ===================================================================================

import Alpine from 'alpinejs'
import { Story } from './codex/story'
import { newlineToBr, niceify } from './utils/strings'
import type { Option } from './codex/option'

let story: Story

Alpine.data('appState', () => ({
  title: '',
  section: {
    id: '',
    title: 'Loading...',
    text: 'Please wait while the story loads...',
    options: new Map<string, Option>(),
  },
  state: {} as Record<string, unknown>,

  loaded: false,
  sheetVisible: true,
  notifyMsg: '',
  confirmMsg: '',
  _confirmResolve: null as ((result: boolean) => void) | null,

  // =====================================================================
  // Application initialization & entry point
  // =====================================================================
  async init() {
    // get story name from query string, default to 'cave'
    const urlParams = new URLSearchParams(window.location.search)
    const storyUrl = urlParams.get('story')
    if (!storyUrl) {
      console.error('No story URL specified')
      return
    }
    console.log(`Hello ${storyUrl}`)

    story = await Story.parse(storyUrl)

    // Find anchor in URL and go there if present
    let startSectionId = 'start'
    const anchor = window.location.hash.slice(1)
    if (anchor) {
      startSectionId = anchor
    }

    if (startSectionId === 'start') {
      localStorage.removeItem('codex_state')
    } else {
      // Load saved state from localStorage
      const savedState = localStorage.getItem('codex_state')
      if (savedState) {
        try {
          const stateObj = JSON.parse(savedState) as Record<string, unknown>
          story.setState(stateObj)
          console.log('Restored saved state from localStorage:', stateObj)
        } catch (e) {
          console.error('Failed to parse saved state from localStorage:', e)
        }
      }
    }

    this.title = story.title
    document.title = this.title
    console.log('Loaded story:', story.title)

    this.gotoSection(startSectionId)
    this.loaded = true

    // Watch for global state changes
    this.$watch('state', (newState) => {
      localStorage.setItem('codex_state', JSON.stringify(newState))
    })
  },

  // =====================================================================
  // Called to navigate to a different section, main state update point
  // =====================================================================
  gotoSection(sectionId: string) {
    if (sectionId === 'restart') {
      localStorage.removeItem('codex_state')
      window.location.href = window.location.href.split('#')[0]
      return
    }

    const section = story.getSection(sectionId)
    if (!section) {
      console.error('Section not found:', sectionId)
      return
    }

    section.visit()
    this.state = story.getState()

    this.section = {
      id: section.id,
      title: niceify(section.id),
      text: newlineToBr(section.text),
      options: section.options,
    }

    // Update URL hash without reloading page
    window.history.replaceState(null, '', `#${sectionId}`)
  },

  // =====================================================================
  // Handle when an option is selected by the user
  // =====================================================================
  async takeOption(option: Option) {
    const result = option.execute()

    if (result.confirmMsg) {
      await this.confirm(result.confirmMsg)
    }

    if (result.nextSectionId) {
      this.gotoSection(result.nextSectionId)
    }

    if (result.notifyMsg) {
      this.notify(result.notifyMsg)
    }
  },

  // =====================================================================
  // Handle interactive event triggering with confirmation
  // =====================================================================
  async triggerEvent(msg: string, eventId: string, ...args: string[]): Promise<void> {
    const res = await this.confirm(`${msg}\n\nAre you sure?`)
    if (!res) {
      return
    }

    const message = story.trigger(eventId, ...args)

    this.state = story.getState()
    this.notify(message)
  },

  // ============ UI HELPERS ==================================================

  notify(message: string) {
    this.notifyMsg = newlineToBr(message)
    ;(this.$refs.notifyDialog as HTMLDialogElement).showModal()
  },

  confirm(message: string): Promise<boolean> {
    this.confirmMsg = newlineToBr(message)
    return new Promise((resolve) => {
      this._confirmResolve = resolve
      ;(this.$refs.confirmDialog as HTMLDialogElement).showModal()
    })
  },

  confirmResult(confirmed: boolean) {
    ;(this.$refs.confirmDialog as HTMLDialogElement).close()
    if (this._confirmResolve) {
      this._confirmResolve(confirmed)
      this._confirmResolve = null
    }
  },

  niceify: niceify,
}))

Alpine.start()
