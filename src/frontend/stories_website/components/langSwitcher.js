import ButtonGroup from '@mui/material/ButtonGroup';
import Button from '@mui/material/Button';
import styles from './app_bar.module.css'
import Link from 'next/link'
import {SUPPORTED_LANGS} from "../src/langService";

export function LangSwitcher({currentLocale, setLocale}) {
    //return generated list of Button with Link with all locales from supportedLangs and links to change locale
    return (
        <ButtonGroup variant="text" aria-label="text button group">
            {SUPPORTED_LANGS.map((locale) => (
                <Button key={locale} className={styles.lang_button}>
                    <Link href="#" onClick={() => setLocale(locale)}>{locale.toUpperCase()}</Link>
                </Button>
            ))}
        </ButtonGroup>
    )
}