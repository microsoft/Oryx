import Link from 'next/link'
import { siteMeta } from '../blog.config'

const Title = ({ path }) => (
  <>
    {path === '/' ? (
      <h1>
        <a href={siteMeta.siteUrl}>{siteMeta.title}</a>
      </h1>
    ) : (
      <p>
        <Link href='/' rel='me'>{siteMeta.title}</Link>
      </p>
    )}
    <style jsx>{`
      h1 {
        margin-top: 0;
      }

      p {
        font-size: 1.2em;
        margin-top: 0;
        font-weight: 500;
      }

      a {
        color: inherit;
        text-decoration: none;
      }

      a:hover {
        text-decoration: underline;
      }
    `}</style>
  </>
)

export default Title