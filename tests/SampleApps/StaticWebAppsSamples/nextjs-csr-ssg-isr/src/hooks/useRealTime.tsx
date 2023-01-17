import { format } from 'date-fns';
import * as React from 'react';

export default function useRealTime() {
  const [realTime, setRealTime] = React.useState<string>(
    format(new Date(), 'kk:mm:ss O')
  );

  React.useEffect(() => {
    const intervalId = setInterval(() => {
      setRealTime(format(new Date(), 'kk:mm:ss O'));
    }, 1000);

    return () => {
      clearInterval(intervalId);
    };
  }, []);

  return realTime;
}
