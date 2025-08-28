import { withRouter } from 'next/router'
import _range from 'lodash.range'
import Link from 'next/link'
import pagination from 'pagination'
import Layout from '../components/layouts/default'
import Post from '../components/blog-index-item'
import blogposts from '../posts/index'
import { siteMeta } from '../blog.config'

const Blog = ({ router, page = 1 }) => {
  const paginator = new pagination.SearchPaginator({
    prelink: '/blog',
    current: page,
    rowsPerPage: siteMeta.postsPerPage,
    totalResult: blogposts.length
  })

  const {
    next,
    previous,
    range,
    fromResult,
    toResult
  } = paginator.getPaginationData()
  const results = blogposts.slice(fromResult - 1, toResult)

  return (
    <Layout>
      <header>
        <h1>{siteMeta.title}</h1>
        <h2>{siteMeta.description}</h2>
      </header>
      {results
        .map(post => (
          <Post
            key={post.title}
            title={post.title}
            summary={post.summary}
            date={post.publishedAt}
            path={post.path}
          />
        ))}

      <ul>
        {previous && (
          <li>
            <Link href={`/blog/${previous}`}>Previous</Link>
          </li>
        )}
        {range.map((page, index) => (
          <li key={index}>
            <Link href={`/blog/${page}`}>{page}</Link>
          </li>
        ))}
        {next && (
          <li>
            <Link href={`/blog/${next}`}>Next</Link>
          </li>
        )}
      </ul>
      <style jsx>{`
        header {
          margin-bottom: 3em;
        }

        h1 {
          font-size: 2em;
        }

        h2 {
          font-weight: 300;
          color: #666;
        }

        ul {
          list-style: none;
          margin: 0;
          padding: 0;
          display: flex;
        }

        li {
          margin-right: 1em;
        }
      `}</style>
    </Layout>
  )
}

export default withRouter(Blog)