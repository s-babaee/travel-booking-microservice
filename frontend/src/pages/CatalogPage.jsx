import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import EmptyState from '../components/EmptyState'
import ErrorNotice from '../components/ErrorNotice'
import LoadingState from '../components/LoadingState'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import { formatMoney, safeArray } from '../lib/formatters'

export default function CatalogPage() {
  const { request } = useAuth()
  const [activeTab, setActiveTab] = useState('hotels')
  const [airlines, setAirlines] = useState([])
  const [routes, setRoutes] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [lookupLoading, setLookupLoading] = useState(false)
  const [error, setError] = useState('')
  const [hotelId, setHotelId] = useState('')
  const [flightId, setFlightId] = useState('')
  const [selectedHotel, setSelectedHotel] = useState(null)
  const [selectedFlight, setSelectedFlight] = useState(null)

  async function loadCatalog() {
    setIsLoading(true)
    setError('')
    try {
      const [airlineResponse, routeResponse] = await Promise.all([
        request('airlines'),
        request('routes')
      ])
      setAirlines(safeArray(airlineResponse))
      setRoutes(safeArray(routeResponse))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadCatalog()
  }, [request])

  async function lookupHotel(event) {
    event.preventDefault()
    if (!hotelId.trim()) return
    setError('')
    setLookupLoading(true)
    try {
      const response = await request(`hotels/${hotelId.trim()}`)
      setSelectedHotel(response)
    } catch (requestError) {
      setSelectedHotel(null)
      setError(requestError.message)
    } finally {
      setLookupLoading(false)
    }
  }

  async function lookupFlight(event) {
    event.preventDefault()
    if (!flightId.trim()) return
    setError('')
    setLookupLoading(true)
    try {
      const response = await request(`flights/${flightId.trim()}`)
      setSelectedFlight(response)
    } catch (requestError) {
      setSelectedFlight(null)
      setError(requestError.message)
    } finally {
      setLookupLoading(false)
    }
  }

  const tabs = [
    ['hotels', 'هتل‌ها'],
    ['flights', 'پروازها'],
    ['airlines', 'شرکت‌های هواپیمایی'],
    ['routes', 'مسیرها']
  ]

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="کاتالوگ سفر"
        title="منابع سفر را پیدا کنید"
        description="اطلاعات کاتالوگ را بررسی کنید و با شناسه‌ی منبع، رزرو جدید بسازید."
        actions={
          <Link to="/app/bookings/new" className="button button-accent">
            رزرو از کاتالوگ <span>←</span>
          </Link>
        }
      />

      <ErrorNotice message={error} onRetry={loadCatalog} />

      <div className="filter-tabs catalog-tabs">
        {tabs.map(([value, label]) => (
          <button
            type="button"
            key={value}
            className={activeTab === value ? 'active' : ''}
            onClick={() => {
              setActiveTab(value)
              setError('')
            }}
          >
            {label}
            {value === 'airlines' ? <span>{airlines.length}</span> : null}
            {value === 'routes' ? <span>{routes.length}</span> : null}
          </button>
        ))}
      </div>

      {isLoading ? (
        <LoadingState />
      ) : (
        <section className="catalog-content">
          {activeTab === 'hotels' ? (
            <div className="lookup-layout">
              <section className="panel lookup-panel">
                <div className="lookup-illustration image-hotel">
                  <span>⌂</span>
                </div>
                <div className="lookup-copy">
                  <span className="eyebrow">Hotel catalog</span>
                  <h2>جزئیات هتل را ببینید</h2>
                  <p>
                    چون API کاتالوگ هتل بر اساس شناسه‌ی هتل طراحی شده، UUID هتل را
                    وارد کنید تا اتاق‌ها، امکانات و سیاست‌ها نمایش داده شوند.
                  </p>
                  <form className="lookup-form" onSubmit={lookupHotel}>
                    <input
                      value={hotelId}
                      onChange={(event) => setHotelId(event.target.value)}
                      placeholder="UUID هتل"
                      dir="ltr"
                      required
                    />
                    <button type="submit" className="button button-dark" disabled={lookupLoading}>
                      {lookupLoading ? 'در حال جست‌وجو...' : 'مشاهده هتل'}
                    </button>
                  </form>
                </div>
              </section>

              {selectedHotel ? (
                <HotelDetails hotel={selectedHotel} />
              ) : (
                <div className="panel lookup-empty">
                  <EmptyState
                    icon="⌂"
                    title="هتلی انتخاب نشده"
                    description="شناسه هتل را وارد کنید تا اتاق‌های قابل رزرو را ببینید."
                  />
                </div>
              )}
            </div>
          ) : null}

          {activeTab === 'flights' ? (
            <div className="lookup-layout">
              <section className="panel lookup-panel">
                <div className="lookup-illustration image-flight">
                  <span>✈</span>
                </div>
                <div className="lookup-copy">
                  <span className="eyebrow">Flight catalog</span>
                  <h2>جزئیات پرواز را ببینید</h2>
                  <p>
                    UUID پرواز را وارد کنید تا برنامه‌ها، کلاس‌های پروازی و
                    سیاست‌های مرتبط را برای شروع رزرو مشاهده کنید.
                  </p>
                  <form className="lookup-form" onSubmit={lookupFlight}>
                    <input
                      value={flightId}
                      onChange={(event) => setFlightId(event.target.value)}
                      placeholder="UUID پرواز"
                      dir="ltr"
                      required
                    />
                    <button type="submit" className="button button-dark" disabled={lookupLoading}>
                      {lookupLoading ? 'در حال جست‌وجو...' : 'مشاهده پرواز'}
                    </button>
                  </form>
                </div>
              </section>

              {selectedFlight ? (
                <FlightDetails flight={selectedFlight} />
              ) : (
                <div className="panel lookup-empty">
                  <EmptyState
                    icon="✈"
                    title="پروازی انتخاب نشده"
                    description="شناسه پرواز را وارد کنید تا کلاس‌های قابل رزرو را ببینید."
                  />
                </div>
              )}
            </div>
          ) : null}

          {activeTab === 'airlines' ? (
            airlines.length ? (
              <div className="catalog-grid">
                {airlines.map((airline) => (
                  <article className="catalog-card airline-card" key={airline.id}>
                    <div className="airline-logo">{airline.iataCode || '✈'}</div>
                    <div className="catalog-card-body">
                      <div className="catalog-card-title">
                        <div>
                          <span className="muted-label">{airline.country}</span>
                          <h3>{airline.name}</h3>
                        </div>
                        <StatusBadge value={airline.status} />
                      </div>
                      <div className="airline-meta">
                        <span><strong>IATA</strong> {airline.iataCode}</span>
                        <span><strong>ICAO</strong> {airline.icaoCode}</span>
                      </div>
                      <div className="catalog-card-footer">
                        <span>{airline.websiteUrl || 'وب‌سایت ثبت نشده'}</span>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            ) : (
              <div className="panel catalog-empty"><EmptyState icon="✈" title="شرکت هواپیمایی ثبت نشده" /></div>
            )
          ) : null}

          {activeTab === 'routes' ? (
            routes.length ? (
              <div className="catalog-grid">
                {routes.map((route) => (
                  <article className="catalog-card route-card-large" key={route.id}>
                    <div className="route-visual">
                      <span>{route.originAirportCode}</span>
                      <span className="route-visual-line">✈</span>
                      <span>{route.destinationAirportCode}</span>
                    </div>
                    <div className="catalog-card-body">
                      <div className="catalog-card-title">
                        <div>
                          <span className="muted-label">مسیر پروازی</span>
                          <h3>{route.originCity} ← {route.destinationCity}</h3>
                        </div>
                        <span className="route-duration">{route.typicalDurationMinutes} دقیقه</span>
                      </div>
                      <p>{route.distanceKm} کیلومتر · شناسه مسیر</p>
                      <div className="catalog-card-footer">
                        <span dir="ltr">{route.id}</span>
                        <button
                          type="button"
                          className="text-link"
                          onClick={() => setActiveTab('flights')}
                        >
                          انتخاب پرواز <span>←</span>
                        </button>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            ) : (
              <div className="panel catalog-empty"><EmptyState icon="⌁" title="مسیری ثبت نشده" /></div>
            )
          ) : null}
        </section>
      )}
    </div>
  )
}

function HotelDetails({ hotel }) {
  const rooms = safeArray(hotel.roomTypes)

  return (
    <section className="panel resource-details">
      <div className="resource-details-header">
        <div>
          <span className="eyebrow">جزئیات هتل</span>
          <h2>{hotel.name}</h2>
          <p>{hotel.city}، {hotel.country} · {hotel.addressLine1}</p>
        </div>
        <StatusBadge value={hotel.status} />
      </div>
      <p className="resource-description">{hotel.description || 'توضیحی برای این هتل ثبت نشده است.'}</p>
      <div className="resource-metrics">
        <div><span>امتیاز</span><strong>{hotel.starRating || 0} ★</strong></div>
        <div><span>امکانات</span><strong>{safeArray(hotel.amenities).length} مورد</strong></div>
        <div><span>سیاست‌ها</span><strong>{safeArray(hotel.policies).length} مورد</strong></div>
      </div>
      <div className="resource-section-heading">
        <h3>اتاق‌های قابل رزرو</h3>
        <span>{rooms.length} نوع</span>
      </div>
      <div className="resource-list">
        {rooms.length ? rooms.map((room) => (
          <div className="resource-row" key={room.id}>
            <div>
              <strong>{room.name}</strong>
              <small>{room.bedType || 'نوع تخت ثبت نشده'} · ظرفیت {room.maxOccupancy} نفر</small>
            </div>
            <span>{room.view || 'نمای معمولی'}</span>
            <Link
              to={`/app/bookings/new?type=hotel&hotelId=${hotel.id}&roomTypeId=${room.id}`}
              className="button button-dark button-small"
            >
              رزرو
            </Link>
          </div>
        )) : (
          <p className="muted-copy">نوع اتاقی برای این هتل ثبت نشده است.</p>
        )}
      </div>
    </section>
  )
}

function FlightDetails({ flight }) {
  const classes = safeArray(flight.classes)
  const schedules = safeArray(flight.schedules)

  return (
    <section className="panel resource-details">
      <div className="resource-details-header">
        <div>
          <span className="eyebrow">جزئیات پرواز</span>
          <h2>پرواز {flight.flightNumber}</h2>
          <p>شناسه پرواز: <span dir="ltr">{flight.id}</span></p>
        </div>
        <StatusBadge value={flight.status} />
      </div>
      <p className="resource-description">{flight.description || flight.aircraftType || 'اطلاعات تکمیلی برای این پرواز ثبت نشده است.'}</p>
      <div className="resource-metrics">
        <div><span>برنامه‌ها</span><strong>{schedules.length} مورد</strong></div>
        <div><span>کلاس‌ها</span><strong>{classes.length} مورد</strong></div>
        <div><span>هواپیما</span><strong>{flight.aircraftType || '—'}</strong></div>
      </div>
      <div className="resource-section-heading">
        <h3>کلاس‌های قابل رزرو</h3>
        <span>{classes.length} کلاس</span>
      </div>
      <div className="resource-list">
        {classes.length ? classes.map((flightClass) => (
          <div className="resource-row" key={flightClass.id}>
            <div>
              <strong>{flightClass.name}</strong>
              <small>{flightClass.code} · ظرفیت {flightClass.capacity} نفر</small>
            </div>
            <span>{formatMoney(flightClass.basePrice, flightClass.currency)}</span>
            <Link
              to={`/app/bookings/new?type=flight&flightId=${flight.id}&flightClassId=${flightClass.id}`}
              className="button button-dark button-small"
            >
              رزرو
            </Link>
          </div>
        )) : (
          <p className="muted-copy">کلاس پروازی برای این پرواز ثبت نشده است.</p>
        )}
      </div>
    </section>
  )
}
