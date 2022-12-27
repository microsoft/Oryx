import React from 'react';

export default function SmallCard({ Icon, title, slug }) {
  return (
    <a className="card-small" href={`/project/${slug}`}>
      <Icon w={153} h={163} />
      <h3>{title}</h3>
    </a>
  );
}
