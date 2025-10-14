import React from 'react'
import Link from 'next/link'
import { parse, format } from 'date-fns'

function PublishedAt(props) {
  const { link, date } = props
  return (
    <>
      <Link href={link} className='u-url' style={{ color: '#aaa' }} {...props}>
        <time className='dt-published'>
          {format(parse(date, 'yyyy-MM-dd', new Date()), 'MMMM dd, yyyy')}
        </time>
      </Link>
      <style jsx>{`
        time {
          color: #aaa;
        }
      `}</style>
    </>
  )
}

export default PublishedAt