import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '@site/src/components/HomepageFeatures';

// Import roadmap
import Image from '@theme/IdealImage';
import bannerImg from './../../static/img/png/roundup/nov.png';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        {/* 
        <h1 className="hero__title">{siteConfig.title}</h1>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        */}
        <h3> #ThisMonthInSWA - Nov 2022! </h3>
        <Link to="blog/roundup-2022-nov">
          <Image img={bannerImg} className={styles.featureImg} />
        </Link>
        <h5>Click the image to read the latest issue!</h5>
      </div>
    </header>
  );
}

/* 
  Was originally with Homepage Header. 
  Removed it as a test 
    <HomepageHeader />
    <main>
      <HomepageFeatures />
    </main>
*/
export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`Hello from ${siteConfig.title}`}
      description="Welcome to the Azure Static Web Apps community site.">
      <main>
        <HomepageHeader />
        <HomepageFeatures />
      </main>
    </Layout>
  );
}
