import { Languages } from 'lucide-react'
import { useTranslation } from 'react-i18next'

const LANGUAGES = ['en', 'vi'] as const

type SupportedLanguage = (typeof LANGUAGES)[number]

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation()

  const currentLanguage = LANGUAGES.includes(i18n.resolvedLanguage as SupportedLanguage)
    ? (i18n.resolvedLanguage as SupportedLanguage)
    : 'en'

  return (
    <label className="inline-flex items-center gap-2 rounded-full border border-gray-200 bg-white px-3 py-1.5 text-xs font-medium text-gray-600">
      <Languages className="h-4 w-4 text-gray-400" />
      <span>{t('language.label')}</span>
      <select
        aria-label={t('language.label')}
        className="bg-transparent text-xs font-medium text-gray-700 outline-none"
        value={currentLanguage}
        onChange={(event) => void i18n.changeLanguage(event.target.value)}
      >
        {LANGUAGES.map((language) => (
          <option key={language} value={language}>
            {t(`language.${language}`)}
          </option>
        ))}
      </select>
    </label>
  )
}
