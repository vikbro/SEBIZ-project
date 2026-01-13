import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import API from '../API/api';

const GamePage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [startTime, setStartTime] = useState<Date | null>(null);
  const gameUrl = location.state?.gameUrl;
  const gameId = location.state?.gameId;

  useEffect(() => {
    if (!gameUrl || !gameId) {
      // Redirect back if the necessary state is not provided
      navigate('/my-library');
      return;
    }

    setStartTime(new Date());

    return () => {
      if (startTime) {
        const endTime = new Date();
        const durationInMinutes = Math.round((endTime.getTime() - startTime.getTime()) / 60000);

        const userString = localStorage.getItem('user');
        if (userString) {
          const user = JSON.parse(userString);
          const data = {
            userId: user.id,
            gameId,
            minutesPlayed: durationInMinutes,
          };
          navigator.sendBeacon('/api/GameUsage/update', JSON.stringify(data));
        }
      }
    };
  }, [gameUrl, gameId, navigate, startTime]);

  if (!gameUrl) {
    return <p>Loading game...</p>;
  }

  return (
    <div style={{ width: '100vw', height: '100vh', position: 'fixed', top: 0, left: 0, zIndex: 1000 }}>
      <iframe
        src={gameUrl}
        title="Game"
        style={{ width: '100%', height: '100%', border: 'none' }}
        sandbox="allow-scripts allow-same-origin"
      />
    </div>
  );
};

export default GamePage;
