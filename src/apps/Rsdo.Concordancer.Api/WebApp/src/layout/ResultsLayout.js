import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import useSearch from '../hooks/use-search';

import History from '../components/shared/History';
import logo from '../assets/logo-small.png';
import styles from './ResultsLayout.module.scss';
import searchIcon from '../assets/search.svg';
import historyIcon from '../assets/history.svg';
import helpIcon from '../assets/help.svg';
import concordanceActiveIcon from '../assets/concordance-active.svg';
import concordanceInactiveIcon from '../assets/concordance-inactive.svg';
import listActiveIcon from '../assets/list-active.svg';
import listInactiveIcon from '../assets/list-inactive.svg';

const ResultsLayout = (props) => {
    // Hooks
    const { t } = useTranslation();
    const search = useSearch();

    // State
    const [query, setQuery] = useState(search.getQuery());
    const [isHelpVisible, setIsHelpVisible] = useState(false);
    const [isHistoryVisible, setIsHistoryVisible] = useState(false);

    // Props
    const { source, help } = props;
    const isConcordance = source === 'concordance';

    const onSubmitHandler = (e) => {
        e.preventDefault();
        if (query) {
            search.newSearch(source, query);
        }
    };

    const onHistoryClickHandler = () => {
        setIsHistoryVisible(isHistoryVisibleOld => !isHistoryVisibleOld);
    }

    const onHelpClickHandler = () => {
        setIsHelpVisible(isHelpVisibleOld => !isHelpVisibleOld);
    }

    return (
        <>
            <div className={styles.menu}>
                <div className='container-fluid'>
                    <div className='row align-items-center'>
                        <div className='col-sm-4 col-lg-2 col-xl-2'>
                            <a href="https://www.slovenscina.eu"><img src={logo} alt='Logo' /></a>
                            <ul className={styles.areaButtons}>
                                <li className={`${styles.areaButton} ${isConcordance ? styles.active : null}`}><Link to={`/${search.corpusId}/concordance`}><img src={isConcordance ? concordanceActiveIcon : concordanceInactiveIcon} alt='Concordance' /></Link></li>
                                <li className={`${styles.areaButton} ${!isConcordance ? styles.active : null}`}><Link to={`/${search.corpusId}/list`}><img src={!isConcordance ? listActiveIcon : listInactiveIcon} alt='List' /></Link></li>
                            </ul>
                        </div>
                        <div className='col-sm-8 col-lg-6 col-xl-4'>
                            <form onSubmit={onSubmitHandler}>
                                <div className={`${styles.searchInput} ${isHelpVisible || isHistoryVisible ? styles.dropdownActive : null}`}>
                                    <button className={styles.searchButton} type='submit'><img src={searchIcon} alt='Search' /></button>
                                    <input type='text' placeholder='Vpišite iskalni niz' value={query} onChange={(e) => setQuery(e.target.value)} />
                                    <button className={styles.historyButton} type='button' onClick={onHistoryClickHandler}><img src={historyIcon} alt='History' /></button>
                                    <button className={styles.helpButton} type='button' onClick={onHelpClickHandler}><img src={helpIcon} alt='Help' /></button>
                                </div>
                            </form>
                            {isHistoryVisible && <History source={source} />}
                            {isHelpVisible && help}
                        </div>
                    </div>
                </div>
            </div>
            <div className={styles.results}>
                <div className='container-fluid'>
                    {props.children}
                </div>
            </div>
            <div className={styles.footer}>
                <div className='container-fluid'>
                    <div className='row align-items-center'>
                        <div className='col'>© Amebis, d. o. o., Kamnik</div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default ResultsLayout;

