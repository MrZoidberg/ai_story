import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import ButtonGroup from '@mui/material/ButtonGroup';
import Button from '@mui/material/Button';
import styles from './app_bar.module.css'
import Link from 'next/link'
import { useRouter } from 'next/router'


export function LangSwitcher() {
    const router = useRouter()
    const supportedLangs = router.locales
    const lang = router.locale || router.defaultLocale

    //return generated list of Button with Link with all locales from supportedLangs and links to change locale
    return (
        <ButtonGroup variant="text" aria-label="text button group">
            {supportedLangs.map((locale) => (
                <Button key={locale} className={styles.lang_button}>
                    <Link href={router.asPath} locale={locale}>{locale.toUpperCase()}</Link>
                </Button>
            ))}
        </ButtonGroup>
    )
}

export default function MyAppBar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="primary">
                <Toolbar>
                    <Box display='flex' flexGrow={1}/>
                    <LangSwitcher />
                </Toolbar>
            </AppBar>
        </Box>
    );
}