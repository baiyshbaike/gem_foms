import darkThemeUrl from 'devextreme/dist/css/dx.fluent.blue.dark.compact.css?url'
import lightThemeUrl from 'devextreme/dist/css/dx.fluent.blue.light.compact.css?url'

const THEME_LINK_ID = 'devextreme-theme'

function currentThemeUrl() {
  return document.documentElement.classList.contains('dark')
    ? darkThemeUrl
    : lightThemeUrl
}

function applyTheme(): Promise<void> {
  const href = currentThemeUrl()
  let link = document.querySelector<HTMLLinkElement>(`#${THEME_LINK_ID}`)

  if (link?.href === new URL(href, window.location.href).href) {
    return Promise.resolve()
  }

  if (!link) {
    link = document.createElement('link')
    link.id = THEME_LINK_ID
    link.rel = 'stylesheet'
    document.head.append(link)
  }

  return new Promise((resolve, reject) => {
    link!.onload = () => resolve()
    link!.onerror = () => reject(new Error('DevExtreme theme could not be loaded.'))
    link!.href = href
  })
}

export async function setupDevExtremeTheme() {
  await applyTheme()

  const observer = new MutationObserver(() => {
    void applyTheme()
  })

  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class'],
  })
}
