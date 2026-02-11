window.glitterBomb = function() {
    const overlay = document.createElement('div');
    overlay.className = 'glitter-bomb-overlay';
    document.body.appendChild(overlay);

    const boom = document.createElement('div');
    boom.className = 'boom-text';
    boom.textContent = '💥 KABOOM! 💥';
    document.body.appendChild(boom);

    const colors = ['#ff6b35', '#ffd700', '#ff1493', '#00ff88', '#00bfff', '#ff4444', '#aa66ff', '#ffaa00'];
    const centerX = window.innerWidth / 2;
    const centerY = window.innerHeight / 2;

    for (let i = 0; i < 60; i++) {
        const particle = document.createElement('div');
        particle.className = 'glitter-particle';
        const angle = (Math.PI * 2 * i) / 60 + (Math.random() - 0.5);
        const distance = 150 + Math.random() * 400;
        const tx = Math.cos(angle) * distance;
        const ty = Math.sin(angle) * distance;
        particle.style.cssText = `
            left: ${centerX}px;
            top: ${centerY}px;
            background: ${colors[i % colors.length]};
            --tx: ${tx}px;
            --ty: ${ty}px;
            width: ${6 + Math.random() * 12}px;
            height: ${6 + Math.random() * 12}px;
            animation-duration: ${0.6 + Math.random() * 0.8}s;
            animation-delay: ${Math.random() * 0.15}s;
            border-radius: ${Math.random() > 0.5 ? '50%' : '2px'};
            box-shadow: 0 0 ${4 + Math.random() * 8}px ${colors[i % colors.length]};
        `;
        overlay.appendChild(particle);
    }

    setTimeout(() => {
        overlay.remove();
        boom.remove();
    }, 2000);
};

window.victoryCelebration = function(taskTitle, contributors) {
    const overlay = document.createElement('div');
    overlay.className = 'glitter-bomb-overlay';
    overlay.style.background = 'rgba(0,0,0,0.4)';
    overlay.style.transition = 'opacity 0.5s';
    document.body.appendChild(overlay);

    const colors = ['#ffd700', '#ff6b35', '#00ff88', '#00bfff', '#ff1493', '#aa66ff', '#ffaa00', '#ff4444'];

    for (let i = 0; i < 100; i++) {
        const confetti = document.createElement('div');
        const x = Math.random() * window.innerWidth;
        const color = colors[Math.floor(Math.random() * colors.length)];
        const size = 6 + Math.random() * 10;
        const duration = 1.5 + Math.random() * 2;
        const delay = Math.random() * 1;
        const rotation = Math.random() * 360;
        confetti.style.cssText = `
            position: fixed;
            left: ${x}px;
            top: -20px;
            width: ${size}px;
            height: ${size * 0.6}px;
            background: ${color};
            border-radius: 2px;
            z-index: 10001;
            pointer-events: none;
            opacity: 0.9;
            transform: rotate(${rotation}deg);
            animation: confetti-fall ${duration}s ease-in ${delay}s forwards;
            box-shadow: 0 0 ${4}px ${color};
        `;
        overlay.appendChild(confetti);
    }

    const banner = document.createElement('div');
    banner.style.cssText = `
        position: fixed;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%) scale(0);
        z-index: 10002;
        text-align: center;
        pointer-events: none;
        animation: victory-banner 3s ease-out forwards;
    `;
    banner.innerHTML = `
        <div style="font-size: 100px; text-shadow: 0 0 30px gold;">🏆</div>
        <div style="font-size: 36px; font-weight: 900; color: #ffd700; text-shadow: 0 0 20px rgba(255,215,0,0.5); margin: 10px 0;">
            MISSION COMPLETE!
        </div>
        <div style="font-size: 20px; color: white; max-width: 500px; margin: 0 auto 15px;">
            "${taskTitle}"
        </div>
        <div style="font-size: 16px; color: #aaa;">
            ${contributors ? '🐧 Team: ' + contributors : '🐧 Great work, penguins!'}
        </div>
    `;
    document.body.appendChild(banner);

    setTimeout(() => {
        overlay.style.opacity = '0';
        banner.style.opacity = '0';
        banner.style.transition = 'opacity 0.5s';
        setTimeout(() => {
            overlay.remove();
            banner.remove();
        }, 600);
    }, 3500);
};
