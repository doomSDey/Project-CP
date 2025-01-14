import React from 'react';

const GameEmbed: React.FC = () => {
  return (
    <div className="flex justify-center items-center bg-black w-full h-full">
      <div className="relative w-full max-w-[1920px] h-0" style={{ paddingTop: '56.25%' }}>
        <iframe
          src="https://project-cp.s3.ap-south-1.amazonaws.com/index.html"
          title="Unity Game"
          className="absolute top-0 left-0 w-full h-full"
          allowFullScreen
        ></iframe>
      </div>
    </div>
  );
};

export default GameEmbed;
