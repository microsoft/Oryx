import * as React from 'react';
import axios from 'axios';
import Seo from '@/components/Seo';
import TimeSection from '@/components/TimeSection';

import { TimeResponse } from '@/types/TimeResponse';

export default function TimePage() {
  const [dateTime, setDateTime] = React.useState<string>();

  React.useEffect(() => {
    axios
      .get<TimeResponse>('https://worldtimeapi.org/api/ip')
      .then((res) => {
        setDateTime(res.data.datetime);
        return res.data.datetime;
      })
      // eslint-disable-next-line no-console
      .catch((error) => console.error(error));
  }, []);

  return (
    <>
      <Seo templateTitle='Time' />

      <main>
        <TimeSection
          title='SSG'
          description='Generated only at build time once.'
          dateTime={dateTime}
        />
      </main>
    </>
  );
}
