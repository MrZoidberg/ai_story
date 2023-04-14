import Head from 'next/head'
import Image from 'next/image'
import styles from './layout.module.css'
//import utilStyles from '../styles/utils.module.css'
import Link from 'next/link'

export const siteTitle = 'RoboStories'

export default function Layout({ children, home }) {
    return (
      <div className={styles.container}>
        <Head>
          <link rel="icon" href="/favicon.ico" />
          <meta
            name="description"
            content="AI generated stories using OpenAI GPT-3"
          />
          <meta name="og:title" content={siteTitle} />
          <meta name="twitter:card" content="summary_large_image" />
        </Head>
        <main>{children}</main>
      </div>
    )
  }