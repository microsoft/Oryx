import axios from 'axios';
import * as React from 'react';
import { GetServerSideProps } from 'next';

import Seo from '@/components/Seo';
import TimeSection from '@/components/TimeSection';

import { TimeResponse } from '@/types/TimeResponse';

type SSRPageProps = {
  dateTime: string;
};

export default function SSRPage({ dateTime }: SSRPageProps) {
  return (
    <>
      <Seo templateTitle='SSR' />

      <main>
        <TimeSection
          title='SSR'
          description='Fetched every render, on server side.'
          dateTime={dateTime}
        />
      </main>
    </>
  );
}

export const getServerSideProps: GetServerSideProps = async () => {
  const res = await axios.get<TimeResponse>('https://worldtimeapi.org/api/ip');

  return {
    props: { dateTime: res.data.datetime },
  };
};
