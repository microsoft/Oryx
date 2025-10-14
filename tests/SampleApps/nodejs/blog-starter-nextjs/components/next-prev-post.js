import PropTypes from 'prop-types'
import Link from 'next/link'

const NextPrevPost = ({ title, path, position }) => {
  const isNext = position === 'next'
  return (
    <>
      <Link href={path}>
        <small>Read {position} post </small>
        {title}
      </Link>
      <style jsx>{`
        a {
          display: flex;
          flex-direction: column;
          ${isNext ? 'text-align: right;' : ''}
          ${isNext ? 'grid-column: 2 / 2;' : ''}
        }

        small {
          color: #999;
        }
      `}</style>
    </>
  )
}

NextPrevPost.propTypes = {
  title: PropTypes.string.isRequired,
  path: PropTypes.string.isRequired,
  position: PropTypes.string.isRequired
}

export default NextPrevPost