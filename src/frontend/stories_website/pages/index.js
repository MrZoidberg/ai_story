import Head from 'next/head';
import * as React from 'react';
import styles from '../styles/Home.module.css';
import Layout, { siteTitle } from '../components/layout';
import StoriesGrid from '../components/stories_grid';
import MyAppBar from '../components/app_bar';
import { storiesFetcher } from '../src/api';
import { SWRConfig } from 'swr';

function Home({fallback}) {
  return (
    <Layout home>
      <Head>
        <title>{siteTitle}</title>
      </Head>
      <MyAppBar />
      <section className={styles.headerSection}>
        <h1 className={styles.title}>
          Welcome to <a href="">AI.Stories!</a>
        </h1>

        <p className={styles.description}>
          Funny stories generated by GPT-3 and other AI models
        </p>
      </section>
      <SWRConfig value={{ fallback }}>
        <StoriesGrid />
      </SWRConfig>
    </Layout>
  )
}


// This function gets called at build time on server-side.
// It won't be called on client-side, so you can even do
// direct database queries.
export async function getStaticProps({ locale }) {  
  const stories = await storiesFetcher(locale, 10, null);

  // By returning { props: { posts } }, the Blog component
  // will receive `posts` as a prop at build time
  return {
    props: {
      fallback: {
        '/api/stories?page=0': stories
      }
    },
  }
}

export default Home