import styles from './IndexMenu.module.scss';
import logo from '../../assets/logo.png';
import langIcon from '../../assets/language.svg';
import { useTranslation } from 'react-i18next';

const IndexTopBar = () => {
    const { i18n } = useTranslation();
    const changeLanguageTitle = i18n.language === 'sl' ? 'English' : 'Slovensko';
    const changeLanguageCode = i18n.language === 'sl' ? 'EN' : 'SL';

    const languageClickHandler = () => {
        const newLanguage = i18n.language === 'sl' ? 'en' : 'sl';
        i18n.changeLanguage(newLanguage);
        localStorage.setItem('language', newLanguage);
    };

    return (
        <div className={styles.menu}>
            <div className='container-fluid'>
                <div className='row align-items-center'>
                    <div className='col'><a href="https://www.slovenscina.eu"><img src={logo} alt='Logo' /></a></div>
                    <div className={`col ${styles.links}`}><button type='button' onClick={languageClickHandler}><img src={langIcon} alt={changeLanguageTitle} />{changeLanguageCode}</button></div>
                </div>
            </div>
        </div>
    );
};

export default IndexTopBar;
