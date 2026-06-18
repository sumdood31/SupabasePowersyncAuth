
CREATE TABLE public.profiles (
    id uuid NOT NULL,
    username text,
    preferences jsonb DEFAULT '{"dark_mode": false, "default_volume": 0.7, "default_ramp_seconds": 60}'::jsonb,
    updated_at timestamp with time zone DEFAULT now(),
    fullname text,
    created_at timestamp with time zone
);


ALTER TABLE public.profiles OWNER TO postgres;

--
-- TOC entry 320 (class 1259 OID 17584)
-- Name: schedules; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.schedules (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    title text,
    type text NOT NULL,
    start_time time without time zone NOT NULL,
    enabled boolean DEFAULT true,
    sound_id uuid,
    volume_max double precision DEFAULT 1.0,
    volume_ramp_duration integer DEFAULT 0,
    reminder_offset_mins integer DEFAULT 15,
    group_id uuid,
    created_at timestamp with time zone DEFAULT now(),
    end_time time without time zone,
    description text,
    days_of_week integer DEFAULT 31 NOT NULL
);


ALTER TABLE public.schedules OWNER TO postgres;


CREATE TABLE public.sounds (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name text NOT NULL,
    origin text DEFAULT 'remote'::text NOT NULL,
    remote_url text,
    local_path text,
    category text
);


ALTER TABLE public.sounds OWNER TO postgres;


--
-- TOC entry 3612 (class 2606 OID 17569)
-- Name: profiles profiles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.profiles
    ADD CONSTRAINT profiles_pkey PRIMARY KEY (id);

--
-- TOC entry 3616 (class 2606 OID 17597)
-- Name: schedules schedules_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedules
    ADD CONSTRAINT schedules_pkey PRIMARY KEY (id);


--
-- TOC entry 3614 (class 2606 OID 17583)
-- Name: sounds sounds_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sounds
    ADD CONSTRAINT sounds_pkey PRIMARY KEY (id);


--
-- TOC entry 3617 (class 2606 OID 17570)
-- Name: profiles profiles_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.profiles
    ADD CONSTRAINT profiles_id_fkey FOREIGN KEY (id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- TOC entry 3618 (class 2606 OID 17603)
-- Name: schedules schedules_sound_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedules
    ADD CONSTRAINT schedules_sound_id_fkey FOREIGN KEY (sound_id) REFERENCES public.sounds(id);


--
-- TOC entry 3619 (class 2606 OID 17598)
-- Name: schedules schedules_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedules
    ADD CONSTRAINT schedules_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.profiles(id) ON DELETE CASCADE;


--
-- TOC entry 3777 (class 3256 OID 17626)
-- Name: schedules Delete own schedules; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Delete own schedules" ON public.schedules FOR DELETE TO authenticated USING ((auth.uid() = user_id));


--
-- TOC entry 3773 (class 3256 OID 17611)
-- Name: sounds Everyone can view sounds; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Everyone can view sounds" ON public.sounds FOR SELECT USING (true);


--
-- TOC entry 3775 (class 3256 OID 17624)
-- Name: schedules Insert own schedules; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Insert own schedules" ON public.schedules FOR INSERT TO authenticated WITH CHECK ((auth.uid() = user_id));


--
-- TOC entry 3776 (class 3256 OID 17625)
-- Name: schedules Update own schedules; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Update own schedules" ON public.schedules FOR UPDATE TO authenticated USING ((auth.uid() = user_id)) WITH CHECK ((auth.uid() = user_id));


--
-- TOC entry 3772 (class 3256 OID 17609)
-- Name: profiles Users can update own profile; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Users can update own profile" ON public.profiles FOR UPDATE USING ((auth.uid() = id));


--
-- TOC entry 3771 (class 3256 OID 17608)
-- Name: profiles Users can view own profile; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "Users can view own profile" ON public.profiles FOR SELECT USING ((auth.uid() = id));


--
-- TOC entry 3774 (class 3256 OID 17623)
-- Name: schedules View own schedules; Type: POLICY; Schema: public; Owner: postgres
--

CREATE POLICY "View own schedules" ON public.schedules FOR SELECT TO authenticated USING ((auth.uid() = user_id));


--
-- TOC entry 3768 (class 0 OID 17561)
-- Dependencies: 318
-- Name: profiles; Type: ROW SECURITY; Schema: public; Owner: postgres
--

ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;

--
-- TOC entry 3770 (class 0 OID 17584)
-- Dependencies: 320
-- Name: schedules; Type: ROW SECURITY; Schema: public; Owner: postgres
--

ALTER TABLE public.schedules ENABLE ROW LEVEL SECURITY;

--
-- TOC entry 3769 (class 0 OID 17575)
-- Dependencies: 319
-- Name: sounds; Type: ROW SECURITY; Schema: public; Owner: postgres
--

ALTER TABLE public.sounds ENABLE ROW LEVEL SECURITY;




