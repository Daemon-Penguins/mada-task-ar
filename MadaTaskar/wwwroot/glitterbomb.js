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
