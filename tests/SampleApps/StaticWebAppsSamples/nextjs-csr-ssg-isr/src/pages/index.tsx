import * as React from 'react';

import useRealTime from '@/hooks/useRealTime';

import Seo from '@/components/Seo';
import CustomLink from '@/components/links/CustomLink';

export default function HomePage() {
  const realTime = useRealTime();
  return (
    <>
      <Seo templateTitle='Home' />

      <main>
        <section className='bg-dark'>
          <div className='flex flex-col items-center justify-center min-h-screen text-center text-white layout'>
            <h1 className='text-2xl md:text-4xl'>
              <CustomLink href='https://github.com/theodorusclarence/nextjs-csr-ssg-isr'>
                Types of Next Rendering
              </CustomLink>
            </h1>
            <p className='mt-2 text-sm text-gray-300'>
              Demo of Next.js rendering type using time API.
            </p>

            <div className='flex flex-col items-center mt-4 space-y-2'>
              <CustomLink href='/render/csr'>CSR</CustomLink>
              <CustomLink href='/render/ssr'>SSR</CustomLink>
              <CustomLink href='/render/ssg'>SSG</CustomLink>
              <CustomLink href='/render/isr'>ISR</CustomLink>
              <CustomLink href='/render/isr-20'>ISR 20s</CustomLink>
            </div>

            <footer className='absolute text-gray-500 bottom-2'>
              © {new Date().getFullYear()} By{' '}
              <CustomLink href='https://theodorusclarence.com?ref=tsnextstarter'>
                Theodorus Clarence
              </CustomLink>
            </footer>
          </div>

          <div className='absolute text-sm font-medium text-right right-4 bottom-8 sm:bottom-4'>
            <p className='text-white'>Real Time:</p>
            <p className='text-primary-400'>{realTime}</p>
          </div>
        </section>
      </main>
    </>
  );
}
