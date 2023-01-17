import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Display from './Display';
import { getUserAsync, selectUserDetails } from './features/user/userSlice';
import TopBar from './TopBar';

function App() {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(getUserAsync());
  }, [dispatch]);

  const userDetails = useSelector(selectUserDetails);

  return (
    <article>
      <TopBar />

        {userDetails === ''
          ? <h4>Login to see todo items</h4>
          : <Display />
        }
    </article>
  );
}

export default App;
