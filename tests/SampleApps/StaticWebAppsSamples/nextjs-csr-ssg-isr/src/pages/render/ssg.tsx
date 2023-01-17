import axios from 'axios';
import * as React from 'react';
import { GetStaticProps } from 'next';

import Seo from '@/components/Seo';
import TimeSection from '@/components/TimeSection';

import { TimeResponse } from '@/types/TimeResponse';

type SSGPageProps = {
  dateTime: string;
};

export default function SSGPage({ dateTime }: SSGPageProps) {
  return (
    <>
      <Seo templateTitle='SSG' />

      <main>
        <TimeSection
          title='SSG'
          description='Fetched only once, when running yarn build on deployment.'
          dateTime={dateTime}
        />
      </main>
    </>
  );
}

export const getStaticProps: GetStaticProps = async () => {
  const res = await axios.get<TimeResponse>('https://worldtimeapi.org/api/ip');

  return {
    props: { dateTime: res.data.datetime },
  };
};
