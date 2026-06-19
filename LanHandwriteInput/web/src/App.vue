<script setup>
import { computed, nextTick, onMounted, ref } from 'vue'

const draft = ref('')
const currentChar = ref('')
const isSending = ref(false)
const isComposing = ref(false)
const hasSendError = ref(false)
const shouldIgnoreNextInput = ref(false)
const queuedChars = ref([])
const toast = ref('')
const inputRef = ref(null)

const pendingCount = computed(() => queuedChars.value.length)
const displayValue = computed(() => draft.value || currentChar.value)
const displayGlyph = computed(() => {
  const value = displayValue.value

  if (!value) return '字'
  if (value === ' ') return '空格'
  if (value === '\b') return '⌫'

  const chars = Array.from(value)
  return chars[chars.length - 1] ?? '字'
})
const isPlaceholder = computed(() => !displayValue.value)
const isCommandDisplay = computed(() => currentChar.value === ' ' || currentChar.value === '\b')
const panelStatus = computed(() => {
  if (hasSendError.value) return 'error'
  if (isSending.value) return 'sending'
  if (toast.value) return 'success'
  return 'idle'
})

async function postText(value) {
  if (!value) return false

  toast.value = '发送中...'
  const response = await fetch('/send', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text: value })
  })

  if (!response.ok) {
    throw new Error('send failed')
  }

  if (value === ' ') {
    toast.value = '已发送空格'
  } else if (value === '\b') {
    toast.value = '已退格'
  } else {
    toast.value = `已发送：${value}`
  }

  return true
}

async function focusInput() {
  await nextTick()
  inputRef.value?.focus({ preventScroll: true })
}

function resetInputElement(inputElement = inputRef.value) {
  draft.value = ''

  if (inputElement) {
    inputElement.value = ''
  }
}

function splitTypedValue(value) {
  return Array.from(value).filter((char) => char !== '\n' && char !== '\r')
}

function enqueueChars(chars) {
  if (chars.length === 0) return

  queuedChars.value.push(...chars)
  hasSendError.value = false
  drainQueue()
}

async function drainQueue() {
  if (isSending.value || queuedChars.value.length === 0) return

  isSending.value = true

  try {
    while (queuedChars.value.length > 0) {
      const value = queuedChars.value[0]
      currentChar.value = value
      await postText(value)
      queuedChars.value.shift()
    }
  } catch {
    hasSendError.value = true
    toast.value = '发送失败'
  } finally {
    isSending.value = false
    focusInput()
  }
}

function consumeInput(event) {
  const value = event.target.value
  const chars = splitTypedValue(value)

  if (chars.length === 0) {
    resetInputElement(event.target)
    return
  }

  currentChar.value = chars[chars.length - 1]
  resetInputElement(event.target)
  enqueueChars(chars)
}

function handleInput(event) {
  if (shouldIgnoreNextInput.value) {
    shouldIgnoreNextInput.value = false
    resetInputElement(event.target)
    return
  }

  draft.value = event.target.value

  if (!isComposing.value) {
    consumeInput(event)
  }
}

function handleCompositionStart() {
  isComposing.value = true
}

function handleCompositionEnd(event) {
  isComposing.value = false
  draft.value = event.target.value

  if (splitTypedValue(event.target.value).length > 0) {
    shouldIgnoreNextInput.value = true
  }

  consumeInput(event)
}

function handleKeydown(event) {
  if (event.key === 'Enter') {
    event.preventDefault()
    resetInputElement(event.currentTarget)
  }

  if (event.key === 'Backspace' && !event.currentTarget.value && !isComposing.value) {
    event.preventDefault()
    sendQuickText('\b')
  }
}

function sendQuickText(value) {
  currentChar.value = value
  enqueueChars([value])
  focusInput()
}

function clearDisplay() {
  draft.value = ''
  currentChar.value = ''
  toast.value = ''
  hasSendError.value = false
  shouldIgnoreNextInput.value = false
  queuedChars.value = []
  resetInputElement()
  focusInput()
}

onMounted(() => {
  focusInput()
})
</script>

<template>
  <main class="page-shell">
    <section class="composer" aria-label="手写输入">
      <h1>手写输入</h1>

      <div class="character-panel" :class="`status-${panelStatus}`" @click="focusInput">
        <div
          class="character-display"
          :class="{ 'is-placeholder': isPlaceholder, 'is-command': isCommandDisplay }"
          aria-hidden="true"
        >
          {{ displayGlyph }}
        </div>
        <input
          ref="inputRef"
          class="character-input"
          autocomplete="off"
          autocapitalize="off"
          autocorrect="off"
          enterkeyhint="done"
          inputmode="text"
          spellcheck="false"
          type="text"
          aria-label="输入一个字"
          :value="draft"
          @compositionstart="handleCompositionStart"
          @compositionend="handleCompositionEnd"
          @input="handleInput"
          @keydown="handleKeydown"
        />
      </div>

      <div class="action-grid three-columns">
        <button class="secondary" type="button" @click="sendQuickText(' ')">空格</button>
        <button class="secondary backspace-button" type="button" title="退格" aria-label="退格" @click="sendQuickText('\b')">
          ⌫
        </button>
        <button class="secondary" type="button" @click="clearDisplay">清屏</button>
      </div>
    </section>
  </main>
</template>
