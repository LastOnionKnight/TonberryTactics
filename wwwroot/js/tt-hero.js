window.ttHero = {
    playHero: function() {
        var host = document.getElementById('heroGauges'); if(!host) return;
        var fills = host.querySelectorAll('.hg-fill'), projs = host.querySelectorAll('.hg-proj');
        var head = document.querySelector('.hero-head');
        var RM = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if(RM){
            fills.forEach(function(f){ f.style.width=f.dataset.fill+'%'; });
            projs.forEach(function(p){ p.style.width=p.dataset.fill+'%'; });
            if(head) head.classList.add('in'); host.classList.add('show-proj'); return;
        }

        fills.forEach(function(f){ f.style.width='0%'; });
        projs.forEach(function(p){ p.style.width='0%'; });
        if(head) head.classList.remove('in'); host.classList.remove('show-proj');

        requestAnimationFrame(function(){
            setTimeout(function(){ fills.forEach(function(f){ f.style.width=f.dataset.fill+'%'; }); }, 160);
            setTimeout(function(){ if(head) head.classList.add('in'); }, 720);
            setTimeout(function(){ projs.forEach(function(p){ p.style.width=p.dataset.fill+'%'; }); host.classList.add('show-proj'); }, 1750);
        });
    },
    buildHero: function() {
        var HERO = [
            { label:'Critical Hit', value:2410, cap:2780, proj:2680 },
            { label:'Direct Hit',   value:1180, cap:2780, proj:1456 }
        ];
        function fmt(n){ return n.toLocaleString('en-US'); }

        var host=document.getElementById('heroGauges'); if(!host) return; host.innerHTML='';
        HERO.forEach(function(s){
            var fillPct=Math.round(s.value/s.cap*100), pPct=Math.round(s.proj/s.cap*100);
            var near=fillPct>=90;
            var gap=s.cap-s.value, closes=s.proj-s.value;
            var g=document.createElement('div'); g.className='hero-gauge';
            g.innerHTML='<div class="hg-head"><span class="hg-label">'+s.label+'</span>'+
            '<span class="hg-val"><span class="cur">'+fmt(s.value)+'</span> <span class="cap">/ '+fmt(s.cap)+' cap</span> <span class="proj">▸ '+fmt(s.proj)+'</span></span></div>'+
            '<div class="hero-track"><div class="hg-proj" data-fill="'+pPct+'"></div>'+
                '<div class="hg-fill'+(near?' is-near':'')+'" data-fill="'+fillPct+'"></div>'+
                '<div class="hero-tick"></div></div>'+
            '<div class="hg-gap">'+fmt(gap)+' to cap · <span class="closes">plan closes '+fmt(closes)+'</span></div>';
            host.appendChild(g);
        });
    },
    init: function() {
        this.buildHero();
        this.playHero();
    }
};
